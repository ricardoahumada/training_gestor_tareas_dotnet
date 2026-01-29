using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManager.Api.Configuration;
using TaskManager.Api.Middleware;
using TaskManager.Application.Extensions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;
using TaskManager.Attachments.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la aplicación
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger
builder.Services.AddSwaggerConfiguration();

// Configuración de CORS
builder.Services.AddCorsConfiguration(builder.Configuration);

// Configuración de Base de Datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("TaskManagerDb");
    // En producción usar SQL Server:
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuración de JWT
var jwtSettings = new JwtSettings
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyAtLeast32CharactersLong!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManager",
    Audience = builder.Configuration["Jwt:Audience"] ?? "TaskManagerClient",
    AccessTokenExpiryMinutes = int.Parse(builder.Configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60"),
    RefreshTokenExpiryDays = int.Parse(builder.Configuration["Jwt:RefreshTokenExpiryDays"] ?? "7")
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

// Inyección de dependencias - Repositorios
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Inyección de dependencias - Servicios
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITaskNotifier, TaskNotifier>();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddSingleton<DateTimeProvider>();

builder.Services.AddMemoryCache();

// Inyección de dependencias - Validadores
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

// Registrar módulo de adjuntos (Strangler Pattern)
builder.Services.AddAttachmentsModule(builder.Configuration);

var app = builder.Build();

// Configuración del pipeline HTTP
app.UseSwaggerConfiguration();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionsMiddleware();
app.UseExecutionTimeLoggerMiddleware();
app.UseLoggingMiddleware();

app.MapControllers();

// Seed data inicial
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Crear usuario admin si no existe
    if (!context.Users.Any(u => u.Username == "admin"))
    {
        context.Users.Add(new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Username = "admin",
            Email = "admin@taskmanager.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

app.Run();

// Clase parcial para permitir tests de integración
public partial class Program { }
