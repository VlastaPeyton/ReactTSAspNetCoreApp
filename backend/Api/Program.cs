using System.Threading.RateLimiting;
using Api.Data;      
using Api.Interfaces;
using Api.Models;
using Api.Repository;
using Api.Service;
using Azure.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Add this using directive
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load(); // loads ".env" from project root, pre svega ostalog jer mnogo toga zavisi od Env.GetString(...) pa ne moze se napravi ako ovo nije omoguceno

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
        { // Mora ova jer OpenApiSecurityRequirement nasledjuje recnik 
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
{   // IdentityRole je AspNetRoles tabela
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true; // Da ne mogu dva usera da imaju isti Email prilikom njihovog dodavanja u AspNetUsers tabelu u Register endpoint

    // AddEntityFrameworkStores tells Identity to use EF to store Identity data (users, roles, tokens, etc.) in the database using your AppDbContext.
}).AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();//  is needed for email confirmation, password reset, etc for ForgotPassword 

// Nakon ova 2 registrovanja iznad, u Package Manager Console kucam "Add-Migration Identity", pa "Update-Database", da sa naprave tabele AspNetUsers, AspNetRoleClaims, AspNetRoles,AspNetUserRoles, AspNetUserClaims... 

// JWT Authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // How app authenticate users (reads JWT)
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // What happens when unathenticated user hits [Authorize]
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme; // What happens when authenticated user is not authorized 
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Fallback if other shcemes arent specified
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme; // For sign in 
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme; // For sign out
    // Svuda koristim JWT Bearer jer to mi najlakse 

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"], // appsettings.json
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
    };
});

/* Add JSON Serialiaziton settings, jer Stock ima List<Comment>, a Comment ima Stock polje koje pokazuje na Stock  i to je circular reference. Pa da ne dodje do problema. 
Isto vazi i za AppUser/Stock - Portfolio. */
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

app.UseHttpsRedirection(); // Forces HTTP to become HTTPS ako FE posalje Request na http://localhsto:5110 umesto https://localhost:7045

// CORS (Browser sequrity feature that restrics pages from making request to different domain that one that served the page). 
app.UseCors(x => x.WithOrigins("http://localhost:3000") // Samo moj Fronted (od svih sajtova na netu) moze slati Request to my Api (Backend ovaj)
                 .AllowAnyMethod() // Allows every Request method (GET,POST,PUT,DELETE...)
                 .AllowAnyHeader() // Allows custom headers, Authorization headers za JWT...
                 .AllowCredentials());

// Enable Authentication + Authorization
app.UseAuthentication(); // Must come before UseAuthorization as it validates user identity when Login/Register
app.UseAuthorization();  // Enforces access rules based on user identity

// Activate Controllers routing.
app.MapControllers();

// Use Rate Limiter on desired Endpoints 
app.UseRateLimiter(); 

app.Run();
