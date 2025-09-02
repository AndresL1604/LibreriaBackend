using Aplication.UsesCases.Proveedores;
using Domain.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("Cors")]
    public class ProveedoresController : ControllerBase
    {
        private readonly ListarProveedores _listar;
        private readonly ObtenerProveedorPorId _getById;
        private readonly ObtenerProveedorPorDocumento _getByDoc;
        private readonly CrearProveedor _crear;
        private readonly ActualizarProveedor _actualizar;
        private readonly EliminarProveedor _eliminar;

        public ProveedoresController(
            ListarProveedores listar,
            ObtenerProveedorPorId getById,
            ObtenerProveedorPorDocumento getByDoc,
            CrearProveedor crear,
            ActualizarProveedor actualizar,
            EliminarProveedor eliminar)
        {
            _listar = listar;
            _getById = getById;
            _getByDoc = getByDoc;
            _crear = crear;
            _actualizar = actualizar;
            _eliminar = eliminar;
        }

        // GET: api/proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetAll()
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _listar.Ejecutar(ct);
            return Ok(list);
        }

        // GET: api/proveedores/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Proveedor>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var entity = await _getById.Ejecutar(id, ct);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        // GET: api/proveedores/by-documento/{documento}
        [HttpGet("by-documento/{documento}")]
        public async Task<ActionResult<Proveedor>> GetByDocumento(string documento)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var entity = await _getByDoc.Ejecutar(documento, ct);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        // POST: api/proveedores
        [HttpPost]
        public async Task<ActionResult<Proveedor>> Create([FromBody] Proveedor request)
        {
            if (request is null) return BadRequest("Body requerido.");
            CancellationToken ct = HttpContext.RequestAborted;

            var creado = await _crear.Ejecutar(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        // PUT: api/proveedores/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Proveedor request)
        {
            if (request is null) return BadRequest("Body requerido.");
            if (id != request.Id) return BadRequest("Id de la URL no coincide con el del cuerpo.");

            CancellationToken ct = HttpContext.RequestAborted;
            var ok = await _actualizar.Ejecutar(id, request, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/proveedores/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var ok = await _eliminar.Ejecutar(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
