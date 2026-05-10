using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using wangluoanqan.Data;
using wangluoanqan.Middleware;
using wangluoanqan.Models;
using wangluoanqan.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<VirusScanService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopment12345678!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "wangluoanqan",
            ValidAudience = "wangluoanqanUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database (for development)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var crypto = scope.ServiceProvider.GetRequiredService<CryptoService>();

    db.Database.EnsureCreated();

    // 添加测试用户（如果不存在）
    if (!db.Users.Any())
    {
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = crypto.HashPassword("Admin123!"),
            Role = "Admin",
            IsMfaEnabled = false,
            FailedAttempts = 0
        };
        var customerUser = new User
        {
            Username = "customer",
            PasswordHash = crypto.HashPassword("Customer123!"),
            Role = "Customer",
            IsMfaEnabled = false,
            FailedAttempts = 0
        };
        db.Users.AddRange(adminUser, customerUser);
        db.SaveChanges();
    }
}

app.Run();