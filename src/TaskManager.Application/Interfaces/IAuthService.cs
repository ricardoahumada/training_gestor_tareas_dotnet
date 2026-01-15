using TaskManager.Application.DTOs;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de autenticación.
    /// </summary>
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterRequest request);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken);
        Task<Result> RevokeTokenAsync(string refreshToken);
    }
    
    /// <summary>
    /// Interfaz para el servicio de validación.
    /// </summary>
    public interface IValidatorService
    {
        Task<ValidationResult> ValidateAsync<T>(T request);
    }
    
    /// <summary>
    /// Resultado de validación.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
