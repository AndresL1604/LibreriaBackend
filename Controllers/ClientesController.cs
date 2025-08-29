using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aplication.UsesCases.Clientes;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ListarClientes _listar;
        private readonly ObtenerClientePorId _getById;
        private readonly ObtenerClientePorDocumento _getByDocumento;
        private readonly CrearCliente _crear;
        private readonly ActualizarCliente _actualizar;
        private readonly EliminarCliente _eliminar;

        public ClientesController(
            ListarClientes listar,
            ObtenerClientePorId getById,
            ObtenerClientePorDocumento getByDocumento,
            CrearCliente crear,
            ActualizarCliente actualizar,
            EliminarCliente eliminar)
        {
            _listar = listar;
            _getById = getById;
            _getByDocumento = getByDocumento;
            _crear = crear;
            _actualizar = actualizar;
            _eliminar = eliminar;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _listar.Ejecutar(ct);
            return Ok(list);
        }

        // GET: api/clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cliente>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var entity = await _getById.Ejecutar(id, ct);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        // GET: api/clientes/by-documento/123456
        [HttpGet("by-documento/{documento}")]
        public async Task<ActionResult<Cliente>> GetByDocumento(string documento)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var entity = await _getByDocumento.Ejecutar(documento, ct);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> Create([FromBody] Cliente request)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var created = await _crear.Ejecutar(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/clientes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Cliente request)
        {
            if (id != request.Id) return BadRequest("Id de la URL no coincide con el del cuerpo.");
            CancellationToken ct = HttpContext.RequestAborted;

            var ok = await _actualizar.Ejecutar(id, request, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/clientes/5
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
