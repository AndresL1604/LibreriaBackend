using Aplication.UsesCases.Alertas;
using Aplication.UsesCases.Clientes;
using Aplication.UsesCases.Compras;
using Aplication.UsesCases.Devoluciones;
using Aplication.UsesCases.Productos;
using Aplication.UsesCases.Promociones;
using Aplication.UsesCases.Proveedores;
using Aplication.UsesCases.Ventas;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;        // <-- IMPORTANTE
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<BdContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) CORS (tus controllers usan [EnableCors("Cors")])
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Cors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 3) Controllers + JSON (ignorar ciclos y omitir nulls)
builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 4) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5) Repositorios (ajusta las implementaciones si tus nombres difieren)
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IDevolucionRepository, DevolucionRepository>();
builder.Services.AddScoped<IPromocionRepository, PromocionRepository>();
builder.Services.AddScoped<IAlertaStockRepository, AlertaStockRepository>();

// 6) UseCases (solo los que ya estamos usando; agrega más cuando refactorices otros controladores)
builder.Services.AddScoped<ListarClientes>();
builder.Services.AddScoped<ObtenerClientePorId>();
builder.Services.AddScoped<ObtenerClientePorDocumento>();
builder.Services.AddScoped<CrearCliente>();
builder.Services.AddScoped<ActualizarCliente>();
builder.Services.AddScoped<EliminarCliente>();

builder.Services.AddScoped<ListarAlertas>();
builder.Services.AddScoped<ListarAlertasPorProducto>();


builder.Services.AddScoped<ListarCompras>();
builder.Services.AddScoped<ObtenerCompraPorId>();
builder.Services.AddScoped<RegistrarCompra>();

builder.Services.AddScoped<ObtenerDevolucionPorId>();
builder.Services.AddScoped<ListarDevolucionesPorVenta>();
builder.Services.AddScoped<RegistrarDevolucion>();

builder.Services.AddScoped<ListarProductos>();
builder.Services.AddScoped<ObtenerProductoPorId>();
builder.Services.AddScoped<CrearProducto>();
builder.Services.AddScoped<ActualizarProducto>();
builder.Services.AddScoped<EliminarODesactivarProducto>();
builder.Services.AddScoped<BuscarProductos>();            // para /search
builder.Services.AddScoped<ListarProductosBajoStock>();   // para /low-stock

builder.Services.AddScoped<ListarPromociones>();
builder.Services.AddScoped<ObtenerPromocionPorId>();
builder.Services.AddScoped<ActualizarPromocion>();
builder.Services.AddScoped<EliminarPromocion>();

builder.Services.AddScoped<CrearPromocion>();
builder.Services.AddScoped<ListarPromocionesVigentes>();
builder.Services.AddScoped<ListarPromocionesPorProducto>();

builder.Services.AddScoped<ListarProveedores>();
builder.Services.AddScoped<ObtenerProveedorPorId>();
builder.Services.AddScoped<ObtenerProveedorPorDocumento>();
builder.Services.AddScoped<CrearProveedor>();
builder.Services.AddScoped<ActualizarProveedor>();
builder.Services.AddScoped<EliminarProveedor>();


builder.Services.AddScoped<ObtenerVentaPorId>();
builder.Services.AddScoped<ListarVentas>();
builder.Services.AddScoped<RegistrarVenta>();


var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Importante: CORS antes de MapControllers
app.UseCors("Cors");

app.MapControllers();

app.Run();
