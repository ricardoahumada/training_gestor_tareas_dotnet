using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    /// <summary>
    /// Validador para solicitudes de creación de tareas.
    /// </summary>
    public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("El título es obligatorio")
                .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");
            
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres");
            
            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("La fecha límite no puede ser anterior a la fecha actual")
                .When(x => x.DueDate.HasValue);
            
            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("La prioridad especificada no es válida");
        }
    }
    
    /// <summary>
    /// Validador para solicitudes de actualización de tareas.
    /// </summary>
    public class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("El título es obligatorio")
                .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");
            
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres");
            
            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("La fecha límite no puede ser anterior a la fecha actual")
                .When(x => x.DueDate.HasValue);
            
            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("La prioridad especificada no es válida");
        }
    }
    
    /// <summary>
    /// Validador para solicitudes de actualización de estado.
    /// </summary>
    public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusRequest>
    {
        public UpdateTaskStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado especificado no es válido");
        }
    }
}
