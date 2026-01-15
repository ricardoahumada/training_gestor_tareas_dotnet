using TaskManager.Application.DTOs;
using TaskManager.Application.Extensions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de usuarios.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <inheritdoc/>
        public async Task<UserResponse?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user?.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<UserResponse?> GetByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user?.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize)
        {
            var pagedResult = await _userRepository.GetAllAsync(page, pageSize);
            
            return new PagedResult<UserResponse>(
                pagedResult.Items.Select(u => u.MapToResponse()).ToList(),
                pagedResult.TotalCount,
                pagedResult.Page,
                pagedResult.PageSize);
        }
    }
}
