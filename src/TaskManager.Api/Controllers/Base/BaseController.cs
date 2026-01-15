using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Controllers.Base
{
    /// <summary>
    /// Controlador base que proporciona funcionalidad compartida.
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger<BaseController> _logger;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el ID del usuario autenticado desde los claims.
        /// </summary>
        protected Guid? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }

        /// <summary>
        /// Obtiene el nombre de usuario autenticado.
        /// </summary>
        protected string? CurrentUsername => User.FindFirst(ClaimTypes.Name)?.Value;

        /// <summary>
        /// Verifica si el usuario tiene un rol específico.
        /// </summary>
        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Crea una respuesta sin contenido.
        /// </summary>
        protected new NoContentResult NoContent()
        {
            return base.NoContent();
        }
    }

    /// <summary>
    /// Respuesta estándar de la API.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
