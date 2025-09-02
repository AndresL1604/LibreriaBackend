using Aplication.DTOs;
using Aplication.UsesCases.Ventas;   // <- use cases
using Domain.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("Cors")]
    public class VentasController : ControllerBase
    {
        private readonly ObtenerVentaPorId _getById;
        private readonly ListarVentas _listar;
        private readonly RegistrarVenta _registrar;

        public VentasController(
            ObtenerVentaPorId getById,
            ListarVentas listar,
            RegistrarVenta registrar)
        {
            _getById = getById;
            _listar = listar;
            _registrar = registrar;
        }

        // GET: api/ventas/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VentaReadDto>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var venta = await _getById.Ejecutar(id, ct);
            if (venta is null) return NotFound();

            var dto = new VentaReadDto
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                Total = venta.Total,
                ClienteId = venta.ClienteId,
                ClienteNombre = venta.Cliente?.Nombre ?? string.Empty,
                Detalles = venta.Detalles.Select(d => new VentaDetalleDto
                {
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/ventas?desde=2025-08-01&hasta=2025-08-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaReadDto>>> List([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var ventas = await _listar.Ejecutar(desde: desde, hasta: hasta, ct: ct);

            var list = ventas
                .OrderByDescending(v => v.Fecha)
                .Select(v => new VentaReadDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Total = v.Total,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.Cliente?.Nombre ?? string.Empty,
                    Detalles = new List<VentaDetalleDto>() // listado liviano
                })
                .ToList();

            return Ok(list);
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<VentaReadDto>> Create([FromBody] VentaCreateDto dto)
        {
            if (dto is null) return BadRequest("Body requerido.");
            if (dto.Items is null || dto.Items.Count == 0) return BadRequest("La venta debe tener al menos un ítem.");
            if (dto.Items.Any(i => i.Cantidad <= 0)) return BadRequest("Todas las cantidades deben ser > 0.");

            CancellationToken ct = HttpContext.RequestAborted;

            // Construimos la entidad que espera el use case
            var venta = new Venta
            {
                ClienteId = dto.ClienteId,
                Fecha = dto.Fecha?.ToUniversalTime() ?? DateTime.UtcNow,
                Total = 0m
            };

            var detalles = dto.Items.Select(i => new DetalleVenta
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario ?? 0m // el use case resolverá precio si UsarPrecioProducto = true
            }).ToList();

            await _registrar.Ejecutar(venta, detalles, usarPrecioProducto: dto.UsarPrecioProducto, ct: ct);

            var created = await _getById.Ejecutar(venta.Id, ct);
            if (created is null) return Problem("Venta creada pero no se pudo leer.");

            var createdDto = new VentaReadDto
            {
                Id = created.Id,
                Fecha = created.Fecha,
                Total = created.Total,
                ClienteId = created.ClienteId,
                ClienteNombre = created.Cliente?.Nombre ?? string.Empty,
                Detalles = created.Detalles.Select(d => new VentaDetalleDto
                {
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }
    }
}
