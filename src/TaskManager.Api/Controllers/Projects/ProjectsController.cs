using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers.Base;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;

namespace TaskManager.Api.Controllers.Projects.v1
{
    /// <summary>
    /// Controlador API REST para la gestión de proyectos.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projects")]
    [Authorize]
    public class ProjectsController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService, ILogger<BaseController> logger)
            : base(logger)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Obtiene un proyecto por su identificador.
        /// </summary>
        /// <param name="id">Identificador del proyecto.</param>
        /// <returns>Información del proyecto.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectResponse>> Get(Guid id)
        {
            var result = await _projectService.GetByIdAsync(id);
            if (result == null)
                return NotFound($"No se encontró un proyecto con el identificador {id}");
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los proyectos con paginación.
        /// </summary>
        /// <param name="page">Número de página.</param>
        /// <param name="pageSize">Tamaño de página.</param>
        /// <returns>Lista paginada de proyectos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProjectResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ProjectResponse>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _projectService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los proyectos del usuario autenticado.
        /// </summary>
        [HttpGet("my-projects")]
        [ProducesResponseType(typeof(PagedResult<ProjectResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ProjectResponse>>> GetMyProjects(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (CurrentUserId == null)
                return Unauthorized();
            var result = await _projectService.GetByOwnerIdAsync(CurrentUserId.Value, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo proyecto.
        /// </summary>
        /// <param name="request">Datos del proyecto.</param>
        /// <returns>Proyecto creado.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
        {
            if (CurrentUserId == null)
                return Unauthorized();
            var result = await _projectService.CreateAsync(request, CurrentUserId.Value);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza un proyecto existente.
        /// </summary>
        /// <param name="id">Identificador del proyecto.</param>
        /// <param name="request">Datos actualizados.</param>
        /// <returns>Proyecto actualizado.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProjectResponse>> Update(Guid id, [FromBody] UpdateProjectRequest request)
        {
            var result = await _projectService.UpdateAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un proyecto (soft delete).
        /// </summary>
        /// <param name="id">Identificador del proyecto.</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _projectService.DeleteAsync(id);
            return NoContent();
        }
    }
}
