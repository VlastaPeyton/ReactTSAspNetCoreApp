using System.Threading.RateLimiting;
using Api.CQRS_and_Validation;
using Api.CQRS_and_Validation.Logging;
using Api.Data;      
using Api.Extensions;
using Api.Interfaces;
using Api.MessageBroker;
using Api.Middlewares;
using Api.Models;
using Api.Repository;
using Api.Service;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

Env.Load(); // Loads ".env" from project root pre svega ostalog, jer mnogo toga (van Program.cs) zavisi od Env.GetString(...)

// Registrovanje servisa u DI - pogledaj Dependency Injection.txt

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
// Nakon ovoga, u Package Manager Console kucam "Add-Migration ime_migracije", pa "Update-Database" da se naprave sve tabele i FK-PK iz OnModelCreating 

// Add IdentityDbContext da bih definisao password kog oblika mora biti i skladistim ga u istu bazu sa Stocks i Comments, stoga mora AddEntityFrameworkStores
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{   // IdentityRole je AspNetRoles tabela. AddIdentity ce napraviti i povezati SVE Identity tabele iz IdentityDbContext, a ne samo ove 2.
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true;         // Da ne mogu dva usera da imaju isti Email prilikom njihovog dodavanja u AspNetUsers tabelu u Register endpoint

}).AddEntityFrameworkStores<ApplicationDBContext>() // AddEntityFrameworkStores tells Identity to use EF Core to store Identity data (users, roles, tokens, etc.) in the database using ApplicationDbContext.
  .AddDefaultTokenProviders();                      // Is needed for email confirmation in Register or password reset in ForgotPassword endpoint

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

// Objasnjeno u JSON engine.txt 
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Kada endpoint vraca odgovor clientu, automatski se pravi JSON i ignorise circular reference
});

// Add MassTransit (MessageBroker + Outbox pattern) za RabbitMQ Publisher - mora pre repository u kom se koristi
builder.Services.AddMassTransitRabbitMQAndOutboxInbox<ApplicationDBContext>(builder.Configuration);

// Svaki servis ima interface zbog SOLID + lako se testira sa xUnit, FakeItEasy/Moq, FluentAssertions itd.

// Add StockRepository i IStockRepository
builder.Services.AddScoped<IStockRepository, StockRepository>();
// Add CachedStockRepository via Scrutor - pogledaj Redis, Proxy & Decorator patterns.txt i pogledaj CachedStockRepository
builder.Services.Decorate<IStockRepository, CachedStockRepository>(); 
// Add IDistributedCache za CachedStockRepository from StackExchangeRedis NuGet kako bih povezao Redis with IDistributedCache
builder.Services.AddStackExchangeRedisCache(config =>
{   
    config.Configuration = builder.Configuration.GetConnectionString("Redis");
});
// Add CommentRepository i ICommentRepository
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// Add TokenService i ITokenService
builder.Services.AddScoped<ITokenService, TokenService>();
// Add IPortfolioRepository i PortfolioRepository 
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
// Add HttpClient for FinancialModelingPrepService 
builder.Services.AddHttpClient<IFinacialModelingPrepService, FinancialModelingPrepService>() // Pogledaj IHttpClientFactory, HttpClient, Resilience.txt
                .AddStandardResilienceHandler(); // Dodaje defaultne retry, timeout, circuit breaker. Pogledaj IHttpClientFactory, HttpClient, Resilience.txt
//builder.Services.AddFMPHttpClientWithCustomResilience();  - ako zelim custom Resilience
// Add EmailService as IEmailSender after Env.Load()
builder.Services.AddScoped<IEmailService, EmailService>();

// Morao sam FluentValidation.DependencyInjectionExtensions da instalim u BuildingBlocks pre ovoga
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly); // Finds CommandValidator klase koje imeplementiraju AbstractValidator , da ValidationBehaviour moze da ih nadje tokom runtime

// Add Mediator
builder.Services.AddMediatR(config =>
{   // Registruje Handler klase jer registruje IRequestHandler koga implementira ICommand/IQueryHandler
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly); // Nadje sve klase koje implementiraju MediatR interface
    // Dodam ValidationBehavior to MediatR pipeline 
    config.AddOpenBehavior(typeof(ValidationBehaviour<,>)); 
    // DOdam LoggingBehavior to MediatR pipeline koje se pokrece automatski 
    config.AddOpenBehavior(typeof(LoggingBehaviour<,>));   
});

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

// Custom middleware koji nasledi IMiddleware mora biti transient
builder.Services.AddTransient<IMiddleware, GlobalExceptionHandlingMiddleware>();

// Options pattern for MessageBrokerSettings
// Registruj u DI IOptions<MessageBrokerSettings> i proveri da l su polja dobro upisana u appsettings.json 
builder.Services.AddOptions<MessageBrokerSettings>()
                .Bind(builder.Configuration.GetSection("MessageBroker"))
                .ValidateDataAnnotations()   
                .ValidateOnStart();
// Zbog ovoga, u MassTransitExtensions.cs moze GetRequiredService<MessageBrokerSettings>, a ne mora GetRequiredService<IOptions<MessageBrokerSettings>>().Value
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

var app = builder.Build();

// Dodavanje middleware u pipeline - pogledaj Middleware.txt 

// Enable Swagger middleware both for Dev and Prod env
app.UseSwagger();
app.UseSwaggerUI(); // Default UI at /swagger

app.UseHttpsRedirection(); // Forces HTTP to become HTTPS ako FE posalje Request na http://localhsto:5110 umesto https://localhost:7045 jer je sigurnije

// CORS (Browser sequrity feature that restrics pages from making request to different domain that one that served the page). 
app.UseCors(x => x.WithOrigins("https://localhost:3000") // Samo moj Fronted(https ili http) (od svih sajtova na netu) moze slati Request to my Api (Backend ovaj)
                 .AllowAnyMethod() // Allows every Request method (GET, POST, PUT, DELETE...)
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
   UseAuthentication hvata Request, iz Request Authorization header ocita JWT (zbog SPA Security best practice JWT is in Auth header)
 ili cookie (ali cookie za AccessToken ne koristim), iz JWT ocita user info (Username i Password) tj Claims, od Claims napravi ClaimsIdentity, 
 od ClaimsIdentity napravi ClaimsPrincipal i upise ga u HttpContext.User.
   ClaimsPrincipal je fakticki user koji moze imati vise ClaimsIdentity npr logovao sam se preko username/password (ClaimsIdentity1) i preko Google naloga(ClaimsIdentity2).
 */
app.UseAuthorization();  
/* Enforces access rules based on user (HttpContext.User koji je UseAuthentication popunio) identity.
   Controller:ControllerBase, a ControllerBase ima User(HttpContext.User) polje koje omogucava stateless da ocitam user info bez gledanja u bazu.
*/

// Use Rate Limiter on desired Endpoints 
app.UseRateLimiter();

// Add custom middleware to pipeline 
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Ubaci Controlers (+ Routing i Endpoint) middleware u pipeline. Za svaki [Http...("route..")] iznad Endpoint ASP.NET Core znace kako da ga mapira sa incoming request.
app.MapControllers();

await app.SeedAdminAsync(); // Admin user se dodaje iz BE uvek

app.Run(); // Middleware dodat u pipeline pomocu app.Run() nema next() 
