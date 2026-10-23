using CVSPharmacy_Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Results;
using CVSPharmacy_Backend.Core.Dtos.Producto;

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;
        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

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
        public async Task<IActionResult> InsertProducto([FromBody] GuardarProductoDto productoDto)
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
