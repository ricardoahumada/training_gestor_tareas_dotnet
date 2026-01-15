using System;
using Microsoft.Extensions.Logging;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones de tareas.
    /// En una aplicación real, esto podría enviar correos electrónicos, notificaciones push, etc.
    /// </summary>
    public class TaskNotifier : ITaskNotifier
    {
        private readonly ILogger<TaskNotifier> _logger;

        public TaskNotifier(ILogger<TaskNotifier> logger)
        {
            _logger = logger;
        }

        public System.Threading.Tasks.Task NotifyTaskAssignedAsync(Domain.Entities.Task task, Guid userId)
        {
            _logger.LogInformation(
                "Notificación: Tarea '{TaskTitle}' (ID: {TaskId}) asignada al usuario {UserId}",
                task.Title,
                task.Id,
                userId);

            // En una implementación real, aquí se enviarían notificaciones
            // Por ejemplo: correo electrónico, notificación push, mensaje a cola, etc.

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task NotifyStatusChangedAsync(Domain.Entities.Task task, Guid userId)
        {
            _logger.LogInformation(
                "Notificación: Tarea '{TaskTitle}' (ID: {TaskId}) cambió estado a {NewStatus}",
                task.Title,
                task.Id,
                task.Status);

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
