using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers.Base;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers.Labels.v1
{
    /// <summary>
    /// Controlador API REST para la gestión de etiquetas.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/labels")]
    [Authorize]
    public class LabelsController : BaseController
    {
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService, ILogger<BaseController> logger)
            : base(logger)
        {
            _labelService = labelService;
        }

        /// <summary>
        /// Obtiene todas las etiquetas disponibles.
        /// </summary>
        /// <returns>Lista de etiquetas.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LabelDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LabelDto>>> GetAll()
        {
            var result = await _labelService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una etiqueta por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la etiqueta.</param>
        /// <returns>Información de la etiqueta.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(LabelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LabelDto>> Get(Guid id)
        {
            var result = await _labelService.GetByIdAsync(id);
            if (result == null)
                return NotFound($"No se encontró una etiqueta con el identificador {id}");
            return Ok(result);
        }

        /// <summary>
        /// Crea una nueva etiqueta (solo administradores).
        /// </summary>
        /// <param name="request">Datos de la etiqueta.</param>
        /// <returns>Etiqueta creada.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LabelDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LabelDto>> Create([FromBody] LabelDto request)
        {
            var result = await _labelService.CreateAsync(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza una etiqueta existente.
        /// </summary>
        /// <param name="id">Identificador de la etiqueta.</param>
        /// <param name="request">Datos actualizados.</param>
        /// <returns>Etiqueta actualizada.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(LabelDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<LabelDto>> Update(Guid id, [FromBody] LabelDto request)
        {
            var result = await _labelService.UpdateAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Elimina una etiqueta.
        /// </summary>
        /// <param name="id">Identificador de la etiqueta.</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _labelService.DeleteAsync(id);
            return NoContent();
        }
    }
}
