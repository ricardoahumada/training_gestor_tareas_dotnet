using TaskManager.Application.DTOs;
using TaskManager.Application.Extensions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de etiquetas.
    /// </summary>
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LabelService(
            ILabelRepository labelRepository,
            ITaskRepository taskRepository,
            IUnitOfWork unitOfWork)
        {
            _labelRepository = labelRepository;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<LabelDto?> GetByIdAsync(Guid id)
        {
            var label = await _labelRepository.GetByIdAsync(id);
            return label != null ? MapToDto(label) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LabelDto>> GetAllAsync()
        {
            var labels = await _labelRepository.GetAllAsync();
            return labels.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<LabelDto> CreateAsync(LabelDto request)
        {
            var label = new Label
            {
                Name = request.Name,
                Color = request.Color,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _labelRepository.AddAsync(label);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(label);
        }

        /// <inheritdoc/>
        public async Task<LabelDto> UpdateAsync(Guid id, LabelDto request)
        {
            var label = await _labelRepository.GetByIdAsync(id);
            if (label == null)
            {
                throw new KeyNotFoundException($"No se encontró una etiqueta con el identificador {id}");
            }

            label.Name = request.Name;
            label.Color = request.Color;
            label.UpdatedAt = DateTime.UtcNow;

            await _labelRepository.UpdateAsync(label);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(label);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var label = await _labelRepository.GetByIdAsync(id);
            if (label == null)
            {
                throw new KeyNotFoundException($"No se encontró una etiqueta con el identificador {id}");
            }

            label.IsActive = false;
            label.UpdatedAt = DateTime.UtcNow;

            await _labelRepository.UpdateAsync(label);
            await _unitOfWork.SaveChangesAsync();
        }

        private static LabelDto MapToDto(Label label)
        {
            return new LabelDto
            {
                Id = label.Id,
                Name = label.Name,
                Color = label.Color
            };
        }
    }
}
