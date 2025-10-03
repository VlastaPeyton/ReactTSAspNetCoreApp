using System.Threading.RateLimiting;
using Api.Data;      
using Api.Interfaces;
using Api.Models;
using Api.Repository;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Add this using directive
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load(); // Loads ".env" from project root pre svega ostalog jer mnogo toga zavisi od Env.GetString(...) pa ne moze se napravi ako ovo nije omoguceno

// Add Controllers classes. Ovo prepoznaje sve iz Controllers foldera.
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add Authorize dugme za unos JWT u Swagger 
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {   
        { // Mora ovaj par zagrada, jer OpenApiSecurityRequirement nasledjuje recnik 
            new OpenApiSecurityScheme // Dictionary Key
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{} // Dictionary Value
        }
    });
});

// Add DbContext 
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});
// Nakon ovoga, u Package Manager Console kucam "Add-Migration CreateTables", pa "Update-Database" da se naprave Stocks, Comments i Portfolios tabele iz ApplicaitonDBContext.cs

// Add IdentityDbContext da bih definisao password kog oblika mora biti i skladistim ga u istu bazu sa Stocks i Comments, stoga mora AddEntityFrameworkStores
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{   // IdentityRole je AspNetRoles tabela. AddIdentity ce napraviti i povezati SVE Identity tabele, a ne samo ove 2.
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true;         // Da ne mogu dva usera da imaju isti Email prilikom njihovog dodavanja u AspNetUsers tabelu u Register endpoint

}).AddEntityFrameworkStores<ApplicationDBContext>() // AddEntityFrameworkStores tells Identity to use EF Core to store Identity data (users, roles, tokens, etc.) in the database using AppDbContext.
  .AddDefaultTokenProviders();                      // Is needed for email confirmation, password reset, etc. for ForgotPassword 

// Nakon ova 2 registrovanja iznad, u Package Manager Console kucam "Add-Migration Identity", pa "Update-Database", da sa naprave sve Identity tabele (objasnjene u ApplicationDbContext.cs) 

// JWT Authentication for [Authorize] endpoints
builder.Services.AddAuthentication(options =>
{   
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // How app authenticate users (reads JWT)
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // What happens when unauthenticated user hits [Authorize] endpoint
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;       // What happens when authenticated user is not authorized 
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;             // Fallback if other shcemes arent specified
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;       // For sign in 
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;      // For sign out
    // Svuda koristim JWT Bearer jer to mi najlakse 

}).AddJwtBearer(options =>
{   /* AddJwtBearer zahteva Authorization = `Bearer ${token}` u React FE, tj BE zahteva JWT u Authorization header of each Request, pa moram jwt token u FE skladistiti u localStorage. 
    Tj svaki endpoint koji ima [Authorize] znaci da mu se moze pristupiti samo ako je user logged in + FE mora mu poslati "Authorization = `Bearer {token}`" u Requst Authorization header.
    Ovo je validacija Access Token (JWT) short-lived, ne Refresh Token !
    Pogledati
    */
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"], // Iz appsettings.json 
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
    };
});

/* Add JSON Serialiaziton settings, jer Stock ima List<Comment>, a Comment ima Stock polje koje pokazuje na Stock i to je circular reference koji dovodi do problema u JSON serialization ako ne ugasim to ovde.
   Isto vazi i za AppUser/Stock - Portfolio.
*/
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Add StockRepository i IStockRepository
builder.Services.AddScoped<IStockRepository, StockRepository>();
// Add CommentRepository i ICommentRepository
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// Add TokenService i ITokenService
builder.Services.AddScoped<ITokenService, TokenService>();
// Add IPortfolioRepository i PortfolioRepository 
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>(); 
// Add FinancialModelingPrepService i IFinancialModelingPrepService
builder.Services.AddScoped<IFinacialModelingPrepService, FinancialModelingPrepService>();
// Add HttpClient for FinancialModelingPrepService
builder.Services.AddHttpClient<IFinacialModelingPrepService, FinancialModelingPrepService>();
// Add EmailService as IEmailSender after Env.Load()
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Rate Limiter 
builder.Services.AddRateLimiter(options =>
{   // Nema globalni default rate limiter => Endpoints koji nemaju [EnableRateLimiting("slow/fast")] nece imati nikakav 
    options.AddFixedWindowLimiter("slow", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5; // 2 Request per 1min . During 1min, if other requests comes after first 5, they will be put in queue, of size QueueuLimit, to wait in QueueProcessingOrder order. Others will be rejected.
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 3;  
    });

    options.AddFixedWindowLimiter("fast", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 7;
    });
});

var app = builder.Build();

// Enable Swagger middleware both for Dev and Prod env
app.UseSwagger();
app.UseSwaggerUI(); // Default UI at /swagger

app.UseHttpsRedirection(); // Forces HTTP to become HTTPS ako FE posalje Request na http://localhsto:5110 umesto https://localhost:7045 jer je sigurnije

// CORS (Browser sequrity feature that restrics pages from making request to different domain that one that served the page). 
app.UseCors(x => x.WithOrigins("https://localhost:3000") // Samo moj Fronted(https ili http) (od svih sajtova na netu) moze slati Request to my Api (Backend ovaj)
                 .AllowAnyMethod() // Allows every Request method (GET,POST,PUT,DELETE...)
                 .AllowAnyHeader() // Allows custom headers, Authorization headers za JWT...
                 .AllowCredentials()); // Da bi FE mogo, kad mu je Access Token blizu isteka , rekao Browseru posalji zahtev za novi Access Token via Cookie(Refresh Token)

/*
app.UseCookiePolicy(new CookiePolicyOptions
{
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always, // Zbog ovoga, mora FE in HTTPS
    MinimumSameSitePolicy = SameSiteMode.None
}); // Jer RefreshToken Endpoint ovo zahteva. Ovo mora before UseAuthentication
Ako ovo imam, onda u AccountController ne pisem Append i ne navodim one parametre.*/

// Enable Authentication + Authorization
app.UseAuthentication(); 
/* UseAuthentication must come before UseAuthorization as it validates user identity when Login/Register.
   UseAuthentication hvat Authorization header iz Request tj hvata JWT koji je uvek u Authorization header smesten u mom slucaju (zbog SPA Security best practice)
 ili cookie (ali cookie JWT mi nije) i napravi ClaimsPrincipal (nasledjen Claim objekat iz ControllerBase) i upise ga u HttpContext.User.
 * */
app.UseAuthorization();  
/* Enforces access rules based on user (HttpContext.User koji je UseAuthentication popunio) identity.
   Controller:ControllerBase, a ControllerBase ima User(HttpContext.User) polje. 
*/

// Use Rate Limiter on desired Endpoints 
app.UseRateLimiter();

// Activate Controllers routing. Za svaki [Http...("route..")] iznad Endpoint ASP.NET Core znace kako da ga mapira sa incoming request.
app.MapControllers();



app.Run();
