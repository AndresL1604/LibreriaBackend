using Aplication.UsesCases.Productos;
using Domain.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("Cors")]
    public class ProductosController : ControllerBase
    {
        private readonly ListarProductos _listar;
        private readonly ObtenerProductoPorId _getById;
        private readonly CrearProducto _crear;
        private readonly ActualizarProducto _actualizar;
        private readonly EliminarODesactivarProducto _eliminarODesactivar;
        private readonly BuscarProductos _buscar;
        private readonly ListarProductosBajoStock _lowStock;

        public ProductosController(
            ListarProductos listar,
            ObtenerProductoPorId getById,
            CrearProducto crear,
            ActualizarProducto actualizar,
            EliminarODesactivarProducto eliminarODesactivar,
            BuscarProductos buscar,
            ListarProductosBajoStock lowStock)
        {
            _listar = listar;
            _getById = getById;
            _crear = crear;
            _actualizar = actualizar;
            _eliminarODesactivar = eliminarODesactivar;
            _buscar = buscar;
            _lowStock = lowStock;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _listar.Ejecutar(ct);
            return Ok(list);
        }

        // GET: api/productos/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Producto>> GetById(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var entity = await _getById.Ejecutar(id, ct);
            if (entity is null) return NotFound();
            return Ok(entity);
        }

        // GET: api/productos/search?texto=lapiz
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Producto>>> Search([FromQuery] string texto)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _buscar.Ejecutar(texto ?? string.Empty, ct);
            return Ok(list);
        }

        // GET: api/productos/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetLowStock()
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var list = await _lowStock.Ejecutar(ct);
            return Ok(list);
        }

        // POST: api/productos
        [HttpPost]
        public async Task<ActionResult<Producto>> Create([FromBody] Producto request)
        {
            if (request is null) return BadRequest("Body requerido.");

            CancellationToken ct = HttpContext.RequestAborted;
            await _crear.Ejecutar(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        // PUT: api/productos/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Producto request)
        {
            if (request is null) return BadRequest("Body requerido.");
            if (id != request.Id) return BadRequest("Id de la URL no coincide con el del cuerpo.");

            CancellationToken ct = HttpContext.RequestAborted;
            var ok = await _actualizar.Ejecutar(request, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/productos/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            CancellationToken ct = HttpContext.RequestAborted;
            var ok = await _eliminarODesactivar.Ejecutar(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
