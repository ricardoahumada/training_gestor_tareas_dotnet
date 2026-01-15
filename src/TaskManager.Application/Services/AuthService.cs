using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using BCrypt.Net;

namespace TaskManager.Application.Services
{
    /// <summary>
    /// Servicio de autenticación JWT.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            JwtSettings jwtSettings)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings;
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<Result> RegisterAsync(RegisterRequest request)
        {
            // Verificar si el usuario ya existe
            if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                return Result.Failure("El nombre de usuario ya está en uso");
            }

            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                return Result.Failure("El correo electrónico ya está registrado");
            }

            // Crear usuario
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            // Buscar por username o email
            var user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail);
            }
            
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<AuthResponse>.Failure("Nombre de usuario o contraseña incorrectos");
            }

            if (!user.IsActive)
            {
                return Result<AuthResponse>.Failure("La cuenta está desactivada");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return Result<AuthResponse>.Success(response);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAllAsync(1, int.MaxValue)
                .ContinueWith(t => t.Result.Items.FirstOrDefault(u => 
                    u.RefreshToken == refreshToken && 
                    u.RefreshTokenExpiry > DateTime.UtcNow));

            if (user == null)
            {
                return Result<AuthResponse>.Failure("Token de refresh inválido o expirado");
            }

            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new AuthResponse
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return Result<AuthResponse>.Success(response);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<Result> RevokeTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAllAsync(1, int.MaxValue)
                .ContinueWith(t => t.Result.Items.FirstOrDefault(u => u.RefreshToken == refreshToken));

            if (user == null)
            {
                return Result.Failure("Token de refresh inválido");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Hashea una contraseña usando BCrypt.
        /// </summary>
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifica una contraseña contra su hash usando BCrypt.
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
