using Aplication.DTOs;
using Aplication.UsesCases.Alertas;
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
    public class AlertasController : ControllerBase
    {
        private readonly ListarAlertas _listar;
        private readonly ListarAlertasPorProducto _listarPorProducto;

        public AlertasController(ListarAlertas listar, ListarAlertasPorProducto listarPorProducto)
        {
            _listar = listar;
            _listarPorProducto = listarPorProducto;
        }

        // GET: api/alertas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertaReadDto>>> List(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var list = await _listar.Ejecutar(desde: desde, hasta: hasta, ct: ct);

            var dto = list
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AlertaReadDto
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    ProductoId = a.ProductoId,
                    ProductoNombre = a.Producto?.Nombre ?? string.Empty,
                    Mensaje = a.Mensaje
                })
                .ToList();

            return Ok(dto);
        }

        // GET: api/alertas/by-producto/{productoId}
        [HttpGet("by-producto/{productoId:int}")]
        public async Task<ActionResult<IEnumerable<AlertaReadDto>>> ByProducto(int productoId)
        {
            CancellationToken ct = HttpContext.RequestAborted;

            var list = await _listarPorProducto.Ejecutar(productoId, ct);

            var dto = list
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AlertaReadDto
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    ProductoId = a.ProductoId,
                    ProductoNombre = a.Producto?.Nombre ?? string.Empty,
                    Mensaje = a.Mensaje
                })
                .ToList();

            return Ok(dto);
        }
    }
}
