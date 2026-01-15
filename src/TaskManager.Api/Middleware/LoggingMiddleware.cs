using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;

namespace TaskManager.Api.Middleware
{
    /// <summary>
    /// Middleware para medición y logging del tiempo de ejecución de requests.
    /// </summary>
    public class ExecutionTimeLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExecutionTimeLoggerMiddleware> _logger;

        public ExecutionTimeLoggerMiddleware(RequestDelegate next, ILogger<ExecutionTimeLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMilliseconds);

                // Advertencia si el tiempo de ejecución es muy largo
                if (elapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMilliseconds);
                }
            }
        }
    }

    /// <summary>
    /// Middleware para logging estructurado de requests y responses.
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.TraceIdentifier;
            
            // Log del request
            _logger.LogInformation(
                "Request {RequestId}: {Method} {Path} {QueryString}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            // Guardar el ID del request para el response
            context.Response.Headers["X-Request-Id"] = requestId;

            await _next(context);

            // Log del response
            _logger.LogInformation(
                "Response {RequestId}: {StatusCode}",
                requestId,
                context.Response.StatusCode);
        }
    }

    /// <summary>
    /// Métodos de extensión para configurar middlewares.
    /// </summary>
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseExecutionTimeLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExecutionTimeLoggerMiddleware>();
        }

        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
