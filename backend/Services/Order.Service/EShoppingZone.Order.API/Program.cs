using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EShoppingZone.Order.API.Data;
using EShoppingZone.Order.API.Repositories;
using EShoppingZone.Order.API.Services;
using EShoppingZone.Order.API.Integrations;
using EShoppingZone.Order.API.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EShoppingZone Order API", Version = "v1" });
    
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
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Clients for inter-service communication
builder.Services.AddTransient<AuthorizationHeaderForwardingHandler>();
builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CartService"] ?? "http://localhost:5003");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthorizationHeaderForwardingHandler>();

builder.Services.AddHttpClient<IWalletServiceClient, WalletServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:WalletService"] ?? "http://localhost:5005");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthorizationHeaderForwardingHandler>();

builder.Services.AddHttpClient<IProfileServiceClient, ProfileServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProfileService"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthorizationHeaderForwardingHandler>();

// JWT Authentication
var jwtSecret = builder.Configuration["JWT__Secret"] ?? "eshoppingzone-super-secret-jwt-key-256-bits-long";
var jwtIssuer = builder.Configuration["JWT__Issuer"] ?? "EShoppingZone";
var jwtAudience = builder.Configuration["JWT__Audience"] ?? "EShoppingZoneUsers";
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
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();

app.Run();