using CVSPharmacy_Backend.Infraestructure.Mappings;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using CVSPharmacy_Backend.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CVSPharmacy_Backend.Services;
using CVSPharmacy_Backend.Services.Validators.Producto;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
using CVSPharmacy_Backend.Services.Service;
using CVSPharmacy_Backend.Api.Filters;
namespace CVSPharmacy_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configurar la BD MySql
            var connectionString = builder.Configuration.GetConnectionString("ConnectionMySql");
            builder.Services.AddDbContext<FarmaciaDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            #endregion
            // Add services to the container.
            #region Inyeccion de Dependencias
            builder.Services.AddScoped(typeof(IBaseRepository<>),typeof(BaseRepository<>));
            builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
            builder.Services.AddScoped<IProveedorService, CVSPharmacy_Backend.Services.ProveedorService>();
            #endregion
            #region Configurar AutoMapper
            builder.Services.AddAutoMapper(typeof(ProductoProfile).Assembly);
            #endregion
            #region Registrar Validadores
            builder.Services.AddScoped<GuardarProductoDtoValidator>();
            builder.Services.AddScoped<GuardarProveedorDtoValidator>();
            builder.Services.AddScoped<CambiarEstadoProductoDtoValidator>();
            builder.Services.AddScoped<CambiarEstadoProveedorDtoValidator>();
            #endregion
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
