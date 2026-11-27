using CVSPharmacy_Backend.Infrastructure.Mappings;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using CVSPharmacy_Backend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CVSPharmacy_Backend.Api.Security;
using CVSPharmacy_Backend.Infrastructure.Security;
using CVSPharmacy_Backend.Services.Validators.Auth;
using CVSPharmacy_Backend.Services.Validators.Usuario;
using CVSPharmacy_Backend.Services.Validators.Producto;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
using CVSPharmacy_Backend.Services.Services;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Api.Filters;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
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
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddSingleton<IVerificadorPassword, VerificadorPassword>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddScoped<CrearUsuarioValidator>();
            builder.Services.AddScoped<EditarUsuarioValidator>();
            builder.Services.AddScoped<LoginDtoValidator>();
            builder.Services.AddScoped<IProductoService, ProductoService>();
            builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
            builder.Services.AddScoped<IProveedorService, ProveedorService>();
            builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            builder.Services.AddScoped<IOrdenCompraService, OrdenCompraService>();
            builder.Services.AddScoped<IRecepcionRepository, RecepcionRepository>();
            builder.Services.AddScoped<IRecepcionService, RecepcionService>();
            #endregion
            #region Configurar AutoMapper
            builder.Services.AddAutoMapper(typeof(ProductoProfile).Assembly);
            #endregion
            #region Registrar Validadores
            builder.Services.AddScoped<GuardarProductoDtoValidator>();
            builder.Services.AddScoped<GuardarProveedorDtoValidator>();
            builder.Services.AddScoped<CambiarEstadoProductoDtoValidator>();
            builder.Services.AddScoped<CambiarEstadoProveedorDtoValidator>();
            builder.Services.AddScoped<GuardarOrdenCompraDtoValidator>();
            builder.Services.AddScoped<GuardarRecepcionDtoValidator>();

            #endregion
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var jwt = new JwtConfig(builder.Configuration);
            builder.Services.AddSingleton(jwt);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = jwt.ParametrosValidacion();
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
