using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// IMPORTANT: Fix Azure Container Apps port issue
// Must match targetPort in Azure (we will use 8080)
builder.WebHost.UseUrls("http://+:8080");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load YARP configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// JWT Configuration
var jwtSecret = builder.Configuration["JWT:Secret"]
    ?? throw new InvalidOperationException("JWT:Secret not configured");

var jwtIssuer = builder.Configuration["JWT:Issuer"]
    ?? throw new InvalidOperationException("JWT:Issuer not configured");

var jwtAudience = builder.Configuration["JWT:Audience"]
    ?? throw new InvalidOperationException("JWT:Audience not configured");

var key = Encoding.ASCII.GetBytes(jwtSecret);

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // IMPORTANT for Azure Container Apps
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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger (always enable for gateway testing)
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EShoppingZone API Gateway v1");
    c.RoutePrefix = "swagger";
});

// IMPORTANT ORDER
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Controllers + Reverse Proxy
app.MapControllers();
app.MapReverseProxy();

app.Run();
public partial class Program { }
