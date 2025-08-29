using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aplication.DTOs;
using Aplication.UsesCases.Compras;   // <-- tus use cases (RegistrarCompra, ListarCompras, ObtenerCompraPorId)
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly RegistrarCompra _registrarCompra;
        private readonly ListarCompras _listarCompras;
        private readonly ObtenerCompraPorId _obtenerCompraPorId;

        public ComprasController(
            RegistrarCompra registrarCompra,
            ListarCompras listarCompras,
            ObtenerCompraPorId obtenerCompraPorId)
        {
            _registrarCompra = registrarCompra;
            _listarCompras = listarCompras;
            _obtenerCompraPorId = obtenerCompraPorId;
        }

        // GET: api/compras/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CompraReadDto>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            // El use case debería traer la compra con detalles y producto (incluidos)
            var compra = await _obtenerCompraPorId.Ejecutar(id, ct);
            if (compra is null) return NotFound();

            var dto = new CompraReadDto
            {
                Id = compra.Id,
                Fecha = compra.Fecha,
                Total = compra.Total,
                ProveedorId = compra.ProveedorId,
                ProveedorNombre = compra.Proveedor?.Nombre ?? string.Empty,
                Detalles = compra.Detalles.Select(d => new CompraDetalleDto
                {
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/compras?desde=2025-08-01&hasta=2025-08-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompraReadDto>>> List(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            // El use case devuelve compras del rango (sin necesidad de detalles)
            var compras = await _listarCompras.Ejecutar(desde: desde, hasta: hasta, ct: ct);

            var list = compras
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CompraReadDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,
                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor?.Nombre ?? string.Empty,
                    // lista liviana en listados
                    Detalles = new List<CompraDetalleDto>()
                })
                .ToList();

            return Ok(list);
        }

        // POST: api/compras
        [HttpPost]
        public async Task<ActionResult<CompraReadDto>> Create([FromBody] CompraCreateDto dto)
        {
            if (dto is null) return BadRequest("Body requerido.");
            if (dto.Items is null || dto.Items.Count == 0) return BadRequest("La compra debe tener al menos un ítem.");

            CancellationToken ct = HttpContext.RequestAborted;

            // Construimos las entidades que espera el use case
            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                Fecha = dto.Fecha?.ToUniversalTime() ?? DateTime.UtcNow,
                Total = 0m,
                Detalles = new List<DetalleCompra>() // opcional; igual pasamos la lista aparte
            };

            var detalles = dto.Items.Select(i => new DetalleCompra
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList();

            // Delegamos toda la lógica al use case (validaciones, total, stock, persistencia)
            await _registrarCompra.Ejecutar(compra, detalles, ct);

            // Volvemos a leer la compra con detalles (use case de obtener)
            var created = await _obtenerCompraPorId.Ejecutar(compra.Id, ct);
            if (created is null) return Problem("La compra fue creada pero no se pudo leer.");

            var createdDto = new CompraReadDto
            {
                Id = created.Id,
                Fecha = created.Fecha,
                Total = created.Total,
                ProveedorId = created.ProveedorId,
                ProveedorNombre = created.Proveedor?.Nombre ?? string.Empty,
                Detalles = created.Detalles.Select(d => new CompraDetalleDto
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
