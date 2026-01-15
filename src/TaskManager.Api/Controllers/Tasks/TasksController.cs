using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers.Base;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Api.Controllers.Tasks.v1
{
    /// <summary>
    /// Controlador API REST para la gestión de tareas del sistema.
    /// Implementa operaciones CRUD completas con filtrado, paginación y ordenamiento.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tasks")]
    public class TasksController : BaseController
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService, ILogger<BaseController> logger)
            : base(logger)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Obtiene una tarea por su identificador único.
        /// </summary>
        /// <param name="id">Identificador GUID de la tarea.</param>
        /// <returns>Información completa de la tarea encontrada.</returns>
        /// <response code="200">Retorna la tarea encontrada.</response>
        /// <response code="404">Tarea no encontrada.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> Get(Guid id)
        {
            var result = await _taskService.GetByIdAsync(id);
            
            if (result == null)
                return NotFound($"No se encontró una tarea con el identificador {id}");
                
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una colección paginada de tareas con opciones de filtrado y ordenamiento.
        /// </summary>
        /// <param name="filter">Criterios de filtrado, paginación y ordenamiento.</param>
        /// <returns>Colección paginada de tareas que coinciden con los criterios.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TaskResponse>>> GetAll([FromQuery] TaskFilterRequest filter)
        {
            var result = await _taskService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todas las tareas asignadas al usuario autenticado.
        /// </summary>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Tamaño de página (por defecto 20).</param>
        /// <returns>Colección de tareas asignadas.</returns>
        [HttpGet("my-tasks")]
        [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TaskResponse>>> GetMyTasks(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            if (CurrentUserId == null)
                return Unauthorized();
                
            var result = await _taskService.GetByAssignedUserIdAsync(CurrentUserId.Value, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las tareas de un proyecto específico.
        /// </summary>
        /// <param name="projectId">Identificador del proyecto.</param>
        /// <param name="page">Número de página.</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <returns>Colección de tareas del proyecto.</returns>
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TaskResponse>>> GetByProject(
            Guid projectId,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var result = await _taskService.GetByProjectIdAsync(projectId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Crea una nueva tarea en el sistema.
        /// </summary>
        /// <param name="request">Datos de la tarea a crear.</param>
        /// <returns>Tarea creada con su información de respuesta.</returns>
        /// <response code="201">Tarea creada exitosamente.</response>
        /// <response code="400">Datos de solicitud inválidos.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request)
        {
            if (CurrentUserId == null)
                return Unauthorized();
                
            var result = await _taskService.CreateAsync(request, CurrentUserId.Value);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza los datos de una tarea existente.
        /// </summary>
        /// <param name="id">Identificador GUID de la tarea.</param>
        /// <param name="request">Datos actualizados de la tarea.</param>
        /// <returns>Tarea con datos actualizados.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaskResponse>> Update(Guid id, [FromBody] UpdateTaskRequest request)
        {
            var result = await _taskService.UpdateAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Actualiza únicamente el estado de una tarea.
        /// </summary>
        /// <param name="id">Identificador GUID de la tarea.</param>
        /// <param name="request">Nuevo estado para la tarea.</param>
        /// <returns>Tarea con estado actualizado.</returns>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaskResponse>> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
        {
            var result = await _taskService.UpdateStatusAsync(id, request.Status);
            return Ok(result);
        }

        /// <summary>
        /// Asigna una tarea a un usuario específico.
        /// </summary>
        /// <param name="id">Identificador GUID de la tarea.</param>
        /// <param name="userId">Identificador del usuario a asignar.</param>
        /// <returns>Tarea con usuario asignado.</returns>
        [HttpPost("{id:guid}/assign/{userId:guid}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<TaskResponse>> AssignTask(Guid id, Guid userId)
        {
            var result = await _taskService.AssignUserAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Elimina lógicamente una tarea del sistema (soft delete).
        /// </summary>
        /// <param name="id">Identificador GUID de la tarea.</param>
        /// <returns>Respuesta sin contenido.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _taskService.DeleteAsync(id);
            return NoContent();
        }
    }
}
