using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers.Base;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Controllers.Auth.v1
{
    /// <summary>
    /// Controlador API REST para autenticación.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, ILogger<BaseController> logger)
            : base(logger)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="request">Datos de registro.</param>
        /// <returns>Resultado del registro.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }

            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                message = "Usuario registrado exitosamente"
            });
        }

        /// <summary>
        /// Inicia sesión y obtiene tokens JWT.
        /// </summary>
        /// <param name="request">Credenciales de inicio de sesión.</param>
        /// <returns>Token de acceso y refresh.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess || result.Data == null)
            {
                return Unauthorized(new { success = false, message = result.ErrorMessage });
            }

            return base.Ok(new
            {
                success = true,
                data = result.Data
            });
        }

        /// <summary>
        /// Renueva el token de acceso usando el refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token actual.</param>
        /// <returns>Nuevo par de tokens.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (!result.IsSuccess || result.Data == null)
            {
                return Unauthorized(new { success = false, message = result.ErrorMessage });
            }

            return base.Ok(new
            {
                success = true,
                data = result.Data
            });
        }

        /// <summary>
        /// Revoca el refresh token actual.
        /// </summary>
        /// <param name="refreshToken">Refresh token a revocar.</param>
        /// <returns>Resultado de la revocación.</returns>
        [HttpPost("revoke")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            var result = await _authService.RevokeTokenAsync(refreshToken);
            if (!result.IsSuccess)
            {
                return BadRequest(new { success = false, message = result.ErrorMessage });
            }

            return base.Ok(new { success = true, message = "Token revocado exitosamente" });
        }

        /// <summary>
        /// Obtiene información del usuario actual.
        /// </summary>
        /// <returns>Información del usuario autenticado.</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            var userResponse = new UserResponse
            {
                Id = CurrentUserId.Value,
                Username = CurrentUsername ?? "Unknown",
                Role = User.FindFirst(ClaimTypes.Role)?.Value == UserRole.Admin.ToString() 
                    ? UserRole.Admin 
                    : UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            return base.Ok(new { success = true, data = userResponse });
        }
    }
}
