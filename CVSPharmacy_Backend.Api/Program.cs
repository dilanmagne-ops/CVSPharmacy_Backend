using CVSPharmacy_Backend.Api.Filters;
using CVSPharmacy_Backend.Api.Identity;
using CVSPharmacy_Backend.Core.CustomEntities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using CVSPharmacy_Backend.Infraestructure.Mappings;
using CVSPharmacy_Backend.Infraestructure.Repositories;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Services;
using CVSPharmacy_Backend.Services.Validators.Producto;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CVSPharmacy_Backend;
public class Program
{
    public static void Main(string[] args)
    {
        CrearAplicacion(args).Run();
    }

    public static WebApplication CrearAplicacion(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("ConnectionMySql")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:ConnectionMySql.");
        builder.Services.AddDbContext<FarmaciaDbContext>(o => o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        builder.Services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IProductoService, ProductoService>();
        builder.Services.AddScoped<IProveedorService, ProveedorService>();
        builder.Services.AddScoped<IOrdenCompraService, OrdenCompraService>();
        builder.Services.AddScoped<IRecepcionService, RecepcionService>();
        builder.Services.AddAutoMapper(typeof(ProductoProfile).Assembly);
        builder.Services.AddScoped<GuardarProductoDtoValidator>();
        builder.Services.AddScoped<CambiarEstadoProductoDtoValidator>();
        builder.Services.AddScoped<GuardarProveedorDtoValidator>();
        builder.Services.AddScoped<CambiarEstadoProveedorDtoValidator>();
        builder.Services.AddScoped<GuardarOrdenCompraDtoValidator>();
        builder.Services.AddScoped<GuardarRecepcionDtoValidator>();
        builder.Services.AddControllers().AddApplicationPart(typeof(Program).Assembly).ConfigureApiBehaviorOptions(o =>
        {
            // Mantener rechazo automático de JSON mal formado y tipos incompatibles.
            o.InvalidModelStateResponseFactory = action => new BadRequestObjectResult(new ErrorResponse
            {
                Status = 400, Title = "BadRequest", Message = "Datos de entrada inválidos.",
                Errors = action.ModelState.Where(e => e.Value?.Errors.Count > 0).Select(e => new
                { field = e.Key, error = "Valor ausente, JSON incorrecto o tipo de dato incompatible." }),
                TraceId = action.HttpContext.TraceIdentifier
            });
        });
        builder.Services.AddOpenApi();
        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
