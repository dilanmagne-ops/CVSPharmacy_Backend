using CVSPharmacy_Backend.Infraestructure.Mappings;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using CVSPharmacy_Backend.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CVSPharmacy_Backend.Services.Validators;
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
            builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
            #endregion
            #region Configurar AutoMapper
            builder.Services.AddAutoMapper(typeof(ProductoProfile).Assembly);
            #endregion
            #region Registrar Validadores
            builder.Services.AddScoped<GuardarProductoDtoValidator>();
            #endregion
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

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
