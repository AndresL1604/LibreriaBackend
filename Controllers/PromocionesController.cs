using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aplication.DTOs;
using Aplication.UsesCases.Promociones;  // <- usa tus use cases
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromocionesController : ControllerBase
    {
        private readonly ListarPromociones _listar;
        private readonly ListarPromocionesVigentes _vigentes;
        private readonly ListarPromocionesPorProducto _porProducto;
        private readonly ObtenerPromocionPorId _getById;
        private readonly CrearPromocion _crear;
        private readonly ActualizarPromocion _actualizar;
        private readonly EliminarPromocion _eliminar;

        public PromocionesController(
            ListarPromociones listar,
            ListarPromocionesVigentes vigentes,
            ListarPromocionesPorProducto porProducto,
            ObtenerPromocionPorId getById,
            CrearPromocion crear,
            ActualizarPromocion actualizar,
            EliminarPromocion eliminar)
        {
            _listar = listar;
            _vigentes = vigentes;
            _porProducto = porProducto;
            _getById = getById;
            _crear = crear;
            _actualizar = actualizar;
            _eliminar = eliminar;
        }

        // GET: api/promociones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromocionReadDto>>> List()
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _listar.Ejecutar(ct);

            var dto = list
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new PromocionReadDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descuento = p.Descuento,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    ProductoId = p.ProductoId,
                    ProductoNombre = p.ProductoId == null ? null : p.Producto?.Nombre
                })
                .ToList();

            return Ok(dto);
        }

        // GET: api/promociones/vigentes?fecha=2025-08-27
        [HttpGet("vigentes")]
        public async Task<ActionResult<IEnumerable<PromocionReadDto>>> Vigentes([FromQuery] DateTime? fecha)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _vigentes.Ejecutar(fecha ?? DateTime.UtcNow, ct);

            var dto = list.Select(p => new PromocionReadDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descuento = p.Descuento,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                ProductoId = p.ProductoId,
                ProductoNombre = p.ProductoId == null ? null : p.Producto?.Nombre
            }).ToList();

            return Ok(dto);
        }

        // GET: api/promociones/by-producto/{productoId}
        [HttpGet("by-producto/{productoId:int}")]
        public async Task<ActionResult<IEnumerable<PromocionReadDto>>> ByProducto(int productoId)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _porProducto.Ejecutar(productoId, ct);

            var dto = list.Select(p => new PromocionReadDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descuento = p.Descuento,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                ProductoId = p.ProductoId,
                ProductoNombre = p.Producto?.Nombre
            }).ToList();

            return Ok(dto);
        }

        // GET: api/promociones/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PromocionReadDto>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var p = await _getById.Ejecutar(id, ct);
            if (p is null) return NotFound();

            var dto = new PromocionReadDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descuento = p.Descuento,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                ProductoId = p.ProductoId,
                ProductoNombre = p.ProductoId == null ? null : p.Producto?.Nombre
            };
            return Ok(dto);
        }

        // POST: api/promociones
        [HttpPost]
        public async Task<ActionResult<PromocionReadDto>> Create([FromBody] PromocionCreateDto dtoIn)
        {
            if (dtoIn is null) return BadRequest("Body requerido.");

            CancellationToken ct = HttpContext.RequestAborted;

            var entity = new Promocion
            {
                Nombre = dtoIn.Nombre,
                Descuento = dtoIn.Descuento,
                FechaInicio = dtoIn.FechaInicio,
                FechaFin = dtoIn.FechaFin,
                ProductoId = dtoIn.ProductoId
            };

            await _crear.Ejecutar(entity, ct);

            // Releer creada
            var created = await _getById.Ejecutar(entity.Id, ct);
            if (created is null) return Problem("Promoción creada pero no se pudo leer.");

            var outDto = new PromocionReadDto
            {
                Id = created.Id,
                Nombre = created.Nombre,
                Descuento = created.Descuento,
                FechaInicio = created.FechaInicio,
                FechaFin = created.FechaFin,
                ProductoId = created.ProductoId,
                ProductoNombre = created.ProductoId == null ? null : created.Producto?.Nombre
            };

            return CreatedAtAction(nameof(GetById), new { id = outDto.Id }, outDto);
        }

        // PUT: api/promociones/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PromocionCreateDto dtoIn)
        {
            if (dtoIn is null) return BadRequest("Body requerido.");
            CancellationToken ct = HttpContext.RequestAborted;

            var ok = await _actualizar.Ejecutar(id, dtoIn.Nombre, dtoIn.Descuento, dtoIn.FechaInicio, dtoIn.FechaFin, dtoIn.ProductoId, ct);
            if (!ok) return NotFound();

            return NoContent();
        }

        // DELETE: api/promociones/{id}
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
