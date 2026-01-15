using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;

namespace TaskManager.Api.Middleware
{
    /// <summary>
    /// Middleware para monitoreo de uso de memoria.
    /// </summary>
    public class MemoryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MemoryMiddleware> _logger;
        private readonly long _memoryThresholdBytes;

        public MemoryMiddleware(RequestDelegate next, ILogger<MemoryMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            var memoryThresholdMb = configuration.GetValue<int>("HealthChecks:MemoryThresholdMB", 500);
            _memoryThresholdBytes = memoryThresholdMb * 1024 * 1024;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var usedMemory = Process.GetCurrentProcess().WorkingSet64;

            if (usedMemory > _memoryThresholdBytes)
            {
                _logger.LogWarning(
                    "High memory usage detected: {UsedMB}MB (threshold: {ThresholdMB}MB)",
                    usedMemory / (1024 * 1024),
                    _memoryThresholdBytes / (1024 * 1024));
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Middleware para aplicar migraciones pendientes al inicio.
    /// </summary>
    public class MigrationsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationsMiddleware> _logger;
        private readonly bool _enabled;

        public MigrationsMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider,
            ILogger<MigrationsMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _enabled = configuration.GetValue<bool>("Database:AutoMigrate");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/" && _enabled)
            {
                _logger.LogInformation("Checking and applying database migrations...");
                
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();
                
                try
                {
                    // Asegurarse de que la base de datos existe
                    await dbContext.Database.EnsureCreatedAsync();
                    _logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying database migrations");
                    throw;
                }
            }

            await _next(context);
        }
    }
}
