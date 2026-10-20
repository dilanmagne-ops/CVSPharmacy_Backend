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

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        private readonly GuardarProductoDtoValidator _validator;

        public ProductoController(IProductoRepository productoRepository, IMapper mapper, GuardarProductoDtoValidator validator)
        {
            _productoRepository = productoRepository;
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
            var productos = await _productoRepository.GetAllProductosAsync();
            if (!string.IsNullOrWhiteSpace(q))
            {
                productos = productos.Where(p => p.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            if (active.HasValue)
            {
                productos = productos.Where(p => p.Activo == active.Value);
            }

            var productosDto = _mapper.Map<IEnumerable<ProductoDto>>(productos);
            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);

            return Ok(response);
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductoById(int id)
        {
            var producto = await _productoRepository
                .GetProductoByIdAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    message = "Producto no encontrado."
                });
            }

            var productoDto = _mapper.Map<ProductoDto>(producto);
            var response = new ApiResponse<ProductoDto>(productoDto);

            return Ok(response);
        }

        // GET: api/products/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetProductosStockCritico()
        {
            var productos = await _productoRepository
                .GetProductosStockCriticoAsync();

            var productosDto = _mapper
                .Map<IEnumerable<ProductoDto>>(productos);

            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDto);

            return Ok(response);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> InsertProducto(
            [FromBody] GuardarProductoDto productoDto)
        {
            var resultado = await _validator.ValidateAsync(productoDto);

            if (!resultado.IsValid)
            {
                return BadRequest(new
                {
                    message = "Error de validacion.",
                    errors = resultado.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            var productoExistente = await _productoRepository
                .GetProductoByCodigoAsync(productoDto.Codigo);

            if (productoExistente != null)
            {
                return Conflict(new
                {
                    message = "El codigo del producto ya existe."
                });
            }

            var producto = _mapper.Map<Producto>(productoDto);

            producto.Activo = true;

            try
            {
                await _productoRepository.InsertProducto(producto);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is MySqlException { Number: 1062 })
            {
                return Conflict(new
                {
                    message = "El codigo del producto ya existe."
                });
            }

            var resultadoDto = _mapper.Map<ProductoDto>(producto);

            var response = new ApiResponse<ProductoDto>(resultadoDto);

            return CreatedAtAction(
                nameof(GetProductoById),
                new { id = producto.Id },
                response);
        }

        // PUT: api/products/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] GuardarProductoDto productoDto)
        {
            var resultado = await _validator.ValidateAsync(productoDto);

            if (!resultado.IsValid)
            {
                return BadRequest(new
                {
                    message = "Error de validacion.",
                    errors = resultado.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            var producto = await _productoRepository
                .GetProductoParaActualizarAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    message = "Producto no encontrado."
                });
            }

            var productoConCodigo = await _productoRepository
                .GetProductoByCodigoAsync(productoDto.Codigo);

            if (productoConCodigo != null &&
                productoConCodigo.Id != id)
            {
                return Conflict(new
                {
                    message = "El codigo pertenece a otro producto."
                });
            }

            _mapper.Map(productoDto, producto);

            try
            {
                await _productoRepository.UpdateProducto(producto);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is MySqlException { Number: 1062 })
            {
                return Conflict(new
                {
                    message = "El codigo pertenece a otro producto."
                });
            }

            var productoActualizado = await _productoRepository.GetProductoByIdAsync(id);

            var resultadoDto = _mapper.Map<ProductoDto>(productoActualizado);
            var response = new ApiResponse<ProductoDto>(resultadoDto);
            return Ok(response);
        }

        // PATCH: api/products/5/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> CambiarEstadoProducto(int id, [FromBody] CambiarEstadoProductoDto estadoDto)
        {
            var producto = await _productoRepository.GetProductoParaActualizarAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    message = "Producto no encontrado."
                });
            }

            producto.Activo = estadoDto.Activo;

            await _productoRepository.UpdateProducto(producto);

            var response = new ApiResponse<object>(new
            {
                message = "Estado actualizado correctamente.",
                id = producto.Id,
                activo = producto.Activo
            });

            return Ok(response);
        }

        #endregion
    }
}
