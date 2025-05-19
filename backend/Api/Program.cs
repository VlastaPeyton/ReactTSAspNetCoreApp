using Api.Data;
using Api.Interfaces;
using Api.Models;
using Api.Repository;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add Controllers 
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
// Nakon ovoga, u Package Manager Console kucam "Add-Migration CreateTables", pa "Update-Database" da se naprave Stocks i Comments tabele

// Add IdentityDbContext da definisem password kog oblika mora biti i skladistim ga u istu bazu kao Stocks i Comments, stoga mora AddEntityFrameworkStores
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true; // Da ne mogu dva usera da imaju isti Email

}).AddEntityFrameworkStores<ApplicationDBContext>();

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
// Nakon ovoga, u Package Manager Console kucam "Add-Migration Identit", pa "Update-Database", da sa naprave AspNetRoleClaims/Roles/UserClaims/Users... tabele

/* Add JSON Serialiaziton settings, jer Stock ima List<Comment>, a Comment ima Stock polje koje pokazuje na Stock klasu i to je circular reference. Pa da ne dodje do problema. 
Isto vazi i za Portfolio i AppUser/Stock. */
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

var app = builder.Build();

// Enable Swagger middleware both for Dev and Prod env
app.UseSwagger();
app.UseSwaggerUI(); // Default UI at /swagger

app.UseHttpsRedirection(); // Forces HTTP -> HTTPS ako frontend pozove http://localhsto:5110 umesto https://localhost:7045

// Enable CORS (Browser sequrity feature that restrics pages from making request to different domain that one that served the page). 
app.UseCors(x => x.WithOrigins("http://localhost:3000") // Samo moj Fronted (od svih sajtova na netu) moze slati Request to my Api (Backend ovaj)
                 .AllowAnyMethod() // Allows GET,POST,PUT,DELETE...
                 .AllowAnyHeader() // Allows custom headers, Authorization...
                 .AllowCredentials());

// Enable Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers 
app.MapControllers();

app.Run();
