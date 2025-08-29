using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aplication.DTOs;
using Aplication.UsesCases.Devoluciones;   // <- usa tus use cases
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevolucionesController : ControllerBase
    {
        private readonly ObtenerDevolucionPorId _getById;
        private readonly ListarDevolucionesPorVenta _listByVenta;
        private readonly RegistrarDevolucion _registrar;

        public DevolucionesController(
            ObtenerDevolucionPorId getById,
            ListarDevolucionesPorVenta listByVenta,
            RegistrarDevolucion registrar)
        {
            _getById = getById;
            _listByVenta = listByVenta;
            _registrar = registrar;
        }

        // GET: api/devoluciones/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DevolucionReadDto>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var dev = await _getById.Ejecutar(id, ct);
            if (dev is null) return NotFound();

            var dto = new DevolucionReadDto
            {
                Id = dev.Id,
                Fecha = dev.Fecha,
                Motivo = dev.Motivo,
                VentaId = dev.VentaId,
                ProductoId = dev.ProductoId,
                ProductoNombre = dev.Producto?.Nombre ?? string.Empty,
                Cantidad = dev.Cantidad
            };

            return Ok(dto);
        }

        // GET: api/devoluciones/by-venta/{ventaId}
        [HttpGet("by-venta/{ventaId:int}")]
        public async Task<ActionResult<IEnumerable<DevolucionReadDto>>> ListByVenta(int ventaId)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var list = await _listByVenta.Ejecutar(ventaId, ct);

            var dto = list
                .OrderByDescending(d => d.Fecha)
                .Select(d => new DevolucionReadDto
                {
                    Id = d.Id,
                    Fecha = d.Fecha,
                    Motivo = d.Motivo,
                    VentaId = d.VentaId,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad
                })
                .ToList();

            return Ok(dto);
        }

        // POST: api/devoluciones
        [HttpPost]
        public async Task<ActionResult<DevolucionReadDto>> Create([FromBody] DevolucionCreateDto dto)
        {
            if (dto is null) return BadRequest("Body requerido.");
            if (string.IsNullOrWhiteSpace(dto.Motivo)) return BadRequest("Motivo es requerido.");
            if (dto.Cantidad <= 0) return BadRequest("Cantidad debe ser > 0.");

            CancellationToken ct = HttpContext.RequestAborted;

            var entity = new Devolucion
            {
                VentaId = dto.VentaId,
                ProductoId = dto.ProductoId,
                Motivo = dto.Motivo,
                Fecha = dto.Fecha?.ToUniversalTime() ?? DateTime.UtcNow,
                Cantidad = dto.Cantidad
            };

            // Lógica de negocio (validaciones, sumar stock, etc.) está dentro del use case
            await _registrar.Ejecutar(entity, ct);

            // Releer para devolver con relaciones (Producto.Nombre)
            var created = await _getById.Ejecutar(entity.Id, ct);
            if (created is null) return Problem("Devolución creada pero no se pudo leer.");

            var createdDto = new DevolucionReadDto
            {
                Id = created.Id,
                Fecha = created.Fecha,
                Motivo = created.Motivo,
                VentaId = created.VentaId,
                ProductoId = created.ProductoId,
                ProductoNombre = created.Producto?.Nombre ?? string.Empty,
                Cantidad = created.Cantidad
            };

            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }
    }
}
