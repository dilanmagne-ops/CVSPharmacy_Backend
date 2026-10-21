using AutoMapper;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Dtos;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Results;

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        private readonly GuardarProductoDtoValidator _validator;
        private readonly IProductoService _productoService;
        private IActionResult? ResponderError<T>(ResultadoServicio<T> resultado)
        {
            switch (resultado.Estado)
            {
                case EstadoResultado.Validacion:
                    return BadRequest(new
                    {
                        message = resultado.Mensaje,
                        errors = resultado.Errores.Select(e => new
                        {
                            field = e.Campo,
                            error = e.Mensaje
                        })
                    });

                case EstadoResultado.NoEncontrado:
                    return NotFound(new
                    {
                        message = resultado.Mensaje
                    });

                case EstadoResultado.Conflicto:
                    return Conflict(new
                    {
                        message = resultado.Mensaje
                    });

                default:
                    return null;
            }
        }

        public ProductoController(
            IProductoRepository productoRepository,
            IProductoService productoService,
            IMapper mapper,
            GuardarProductoDtoValidator validator)
        {
            _productoRepository = productoRepository;
            _productoService = productoService;
            _mapper = mapper;
            _validator = validator;
        }
        #region Sin DTOs - Demostracion

        // GET: api/products/demo/sin-dto/MED-0001
        [HttpGet("demo/sin-dto/{codigo}")]
        public async Task<IActionResult> GetProductoSinDto(string codigo)
        {
            var producto = await _productoRepository
                .GetProductoByCodigoAsync(codigo);

            if (producto == null)
                return NotFound("Producto no encontrado.");

            return Ok(producto);
        }

        #endregion
        #region Con DTOs - Mapeo manual

        // GET: api/products/demo/dto/5
        [HttpGet("demo/dto/{id:int}")]
        public async Task<IActionResult> GetProductoDto(int id)
        {
            var producto = await _productoRepository
                .GetProductoByIdAsync(id);

            if (producto == null)
                return NotFound("Producto no encontrado.");

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

            int stock = producto.Lotes
                .Where(l => l.FechaVencimiento >= hoy
                         && l.CantidadDisponible > 0)
                .Sum(l => l.CantidadDisponible);

            var productoDto = new ProductoDto
            {
                Id = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                PrincipioActivo = producto.PrincipioActivo,
                Presentacion = producto.Presentacion,
                UnidadMedida = producto.UnidadMedida,
                PrecioVenta = producto.PrecioVenta,
                StockMinimo = producto.StockMinimo,
                Activo = producto.Activo,
                CreadoEn = producto.CreadoEn,
                StockDisponible = stock,
                EsStockCritico = stock <= producto.StockMinimo
            };

            return Ok(productoDto);
        }
        #endregion
        #region DTO-AutoMapper - Endpoints oficiales CU-03

        // GET: api/products?q=paracetamol&active=true
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] string? q, [FromQuery] bool? active)
        {
            var productosDto = await _productoService.GetAllProductosAsync(q, active);
            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);
            return Ok(response);
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductoById(int id)
        {
            var productoDto = await _productoService.GetProductoByIdAsync(id);
            if (productoDto == null)
            {
                return NotFound(new
                {
                    message = "Producto no encontrado."
                });
            }
            var response = new ApiResponse<ProductoDto>(productoDto);
            return Ok(response);
        }

        // GET: api/products/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetProductosStockCritico()
        {
            var productosDto = await _productoService.GetProductosStockCriticoAsync();
            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);
            return Ok(response);
        }

        // POST: api/products

        [HttpPost]
        public async Task<IActionResult> InsertProducto(
            [FromBody] GuardarProductoDto productoDto)
        {
            var resultado = await _productoService.CrearProductoAsync(productoDto);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }
            var response = new ApiResponse<ProductoDto>(resultado.Data!);
            return CreatedAtAction(
                nameof(GetProductoById),
                new { id = resultado.Data!.Id },
                response);
        }


        // PUT: api/products/5

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProducto(
            int id,
            [FromBody] GuardarProductoDto productoDto)
        {
            var resultado = await _productoService
                .ActualizarProductoAsync(id, productoDto);

            var error = ResponderError(resultado);

            if (error != null)
            {
                return error;
            }

            var response = new ApiResponse<ProductoDto>(resultado.Data!);

            return Ok(response);
        }


        // PATCH: api/products/5/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> CambiarEstadoProducto(int id, [FromBody] CambiarEstadoProductoDto estadoDto)
        {
            var resultado = await _productoService.CambiarEstadoProductoAsync(id, estadoDto);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }
            var response = new ApiResponse<object>(new
            {
                message = "Estado actualizado correctamente.",
                id = id,
                activo = resultado.Data
            });
            return Ok(response);
        }

        #endregion
    }
}
