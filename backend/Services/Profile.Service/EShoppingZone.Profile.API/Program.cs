using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Text;
using EShoppingZone.Profile.API.Data;
using EShoppingZone.Profile.API.Repositories;
using EShoppingZone.Profile.API.Services;
using EShoppingZone.Profile.API.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EShoppingZone Profile API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Password Hasher
builder.Services.AddScoped<IPasswordHasher<UserProfile>, PasswordHasher<UserProfile>>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Dependency Injection
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured in .env file");
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer not configured in .env file");
var jwtAudience = builder.Configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience not configured in .env file");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// IMPORTANT: Fix Azure Container Apps port issue
builder.WebHost.UseUrls("http://+:8080");

var app = builder.Build();

// Configure the HTTP request pipeline
// Always enable Swagger for debugging in Azure
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EShoppingZone Profile API v1");
    c.RoutePrefix = "swagger"; // Ensure it's at /swagger
});

// app.UseHttpsRedirection(); // Removed for Azure Container Apps TLS termination
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();

app.Run();
public partial class Program { }
