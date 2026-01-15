using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Interfaces;

namespace TaskManager.Application.Services
{
    /// <summary>
    /// Servicio de validación con FluentValidation.
    /// </summary>
    public class ValidatorService : IValidatorService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task<ValidationResult> ValidateAsync<T>(T request)
        {
            var validator = _serviceProvider.GetRequiredService<IValidator<T>>();
            var result = await validator.ValidateAsync(request);

            return new ValidationResult
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => e.ErrorMessage)
            };
        }
    }
}
