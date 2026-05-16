using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EShoppingZone.Cart.API.Data;
using EShoppingZone.Cart.API.Repositories;
using EShoppingZone.Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EShoppingZone Cart API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<CartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// In-Memory Distributed Cache Configuration (replaces Redis)
builder.Services.AddDistributedMemoryCache();

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

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Dependency Injection
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EShoppingZone Cart API v1");
    c.RoutePrefix = "swagger";
});

// app.UseHttpsRedirection(); // Removed for Azure Container Apps TLS termination
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();

app.Run();
public partial class Program { }
