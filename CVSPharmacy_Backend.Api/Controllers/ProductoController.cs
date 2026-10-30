using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Producto;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly IUsuarioActual _usuario;
        public ProductoController(IProductoService productoService, IUsuarioActual usuario)
        {
            _productoService = productoService;
            _usuario = usuario;
        }
        #region DTO-AutoMapper - Endpoints oficiales CU-03

        // GET: api/products?q=paracetamol&active=true
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] string? q, [FromQuery] bool? active)
        {
            await _usuario.ObtenerAsync();
            var productosDto = await _productoService.GetAllProductosAsync(q, active);
            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);
            return Ok(response);
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductoById(int id)
        {
            await _usuario.ObtenerAsync();
            var productoDto = await _productoService.GetProductoByIdAsync(id);
            var response = new ApiResponse<ProductoDto>(productoDto);
            return Ok(response);
        }

        // GET: api/products/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetProductosStockCritico()
        {
            await _usuario.ObtenerAsync();
            var productosDto = await _productoService.GetProductosStockCriticoAsync();
            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);
            return Ok(response);
        }

        // POST: api/products

        [HttpPost]
        public async Task<IActionResult> InsertProducto([FromBody] GuardarProductoDto productoDto)
        {
            await _usuario.ObtenerAsync();
            var producto = await _productoService.CrearProductoAsync(productoDto);
            var response = new ApiResponse<ProductoDto>(producto);
            return CreatedAtAction(
              nameof(GetProductoById),
              new { id = producto.Id },
              response);
        }


        // PUT: api/products/5

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProducto(
            int id,
            [FromBody] GuardarProductoDto productoDto)
        {
            await _usuario.ObtenerAsync();
            var producto = await _productoService
                .ActualizarProductoAsync(id, productoDto);

            var response = new ApiResponse<ProductoDto>(producto);

            return Ok(response);
        }


        // PATCH: api/products/5/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> CambiarEstadoProducto(int id, [FromBody] CambiarEstadoProductoDto estadoDto)
        {
            await _usuario.ObtenerAsync(true);
            var activo = await _productoService.CambiarEstadoProductoAsync(id, estadoDto);
            var response = new ApiResponse<object>(new
            {
                message = "Estado actualizado correctamente.",
                id = id,
                activo = activo
            });

            return Ok(response);
        }

        #endregion
    }
}
