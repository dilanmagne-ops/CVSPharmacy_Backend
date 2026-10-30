using CVSPharmacy_Backend.Services.Interfaces;
using AutoMapper;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Results;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Producto;
using CVSPharmacy_Backend.Core.Dtos.Producto;

namespace CVSPharmacy_Backend.Services.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IBaseRepository<Producto> _productoRepository;
        private readonly IMapper _mapper;
        private readonly GuardarProductoDtoValidator _validator;
        private readonly CambiarEstadoProductoDtoValidator _estadoValidator;

        public ProductoService(IBaseRepository<Producto> productoRepository, IMapper mapper, GuardarProductoDtoValidator validator, CambiarEstadoProductoDtoValidator estadoValidator)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
            _validator = validator;
            _estadoValidator = estadoValidator;
        }
        public async Task<IEnumerable<ProductoDto>> GetAllProductosAsync(string? q, bool? active)
        {
            IEnumerable<Producto> productos = await _productoRepository.GetAllAsync(null, p => p.Lotes);
            if (!string.IsNullOrWhiteSpace(q))
            {
                productos = productos.Where(p => p.Codigo.Contains(q, StringComparison.OrdinalIgnoreCase) 
                || p.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase));
            }
            if (active.HasValue)
            {
                productos = productos.Where(p => p.Activo == active.Value);
            }

            return _mapper.Map<IEnumerable<ProductoDto>>(productos);
        }

        public async Task<ProductoDto?> GetProductoByIdAsync(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id, p => p.Lotes);
            if (producto == null)
                return null;

            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<IEnumerable<ProductoDto>> GetProductosStockCriticoAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var productos = await _productoRepository.GetAllAsync(p =>
                (p.Lotes.Where(l => l.FechaVencimiento >= hoy && l.CantidadDisponible > 0)
                    .Sum(l => (int?)l.CantidadDisponible) ?? 0) <= p.StockMinimo,
                p => p.Lotes);
            return _mapper.Map<IEnumerable<ProductoDto>>(productos);
        }
        private async Task<ResultadoServicio<ProductoDto>?> ValidarProductoAsync(GuardarProductoDto productoDto)
        {
            var resultado = await _validator.ValidateAsync(productoDto);

            if (resultado.IsValid)
            {
                return null;
            }

            return new ResultadoServicio<ProductoDto>
            {
                Estado = EstadoResultado.Validacion,
                Mensaje = "Error de validacion.",
                Errores = resultado.Errors.Select(e => new ErrorCampo
                {
                    Campo = e.PropertyName,
                    Mensaje = e.ErrorMessage
                }).ToList()
            };
        }
        public async Task<ResultadoServicio<ProductoDto>> CrearProductoAsync(GuardarProductoDto productoDto)
        {
            // Validar los datos
            var validacion = await ValidarProductoAsync(productoDto);
            if (validacion != null)
            {
                return validacion;
            }
            // Verificar codigo duplicado
            var productoExistente = await _productoRepository.FirstOrDefaultAsync(p => p.Codigo == productoDto.Codigo);
            if (productoExistente != null)
            {
                return new ResultadoServicio<ProductoDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = "El codigo del producto ya existe."
                };
            }
            // Convertir DTO a entidad
            var producto = _mapper.Map<Producto>(productoDto);
            producto.Activo = true;
            try
            {
                await _productoRepository.AddAsync(producto);
                await _productoRepository.SaveAsync();
            }
            catch (DuplicateKeyException)
            {
                return new ResultadoServicio<ProductoDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = "El codigo del producto ya existe."
                };
            }

            // Consultar el producto recien registrado
            var productoGuardado = await _productoRepository.GetByIdAsync(producto.Id, p => p.Lotes);
            var resultadoDto = _mapper.Map<ProductoDto>(productoGuardado ?? producto);
            return new ResultadoServicio<ProductoDto>
            {
                Estado = EstadoResultado.Exito,
                Data = resultadoDto
            };
        }
        //PUT
        public async Task<ResultadoServicio<ProductoDto>> ActualizarProductoAsync(int id, GuardarProductoDto productoDto)
        {
            // Validar los datos
            var validacion = await ValidarProductoAsync(productoDto);

            if (validacion != null)
            {
                return validacion;
            }

            // Buscar producto existente
            var producto = await _productoRepository
                .GetForUpdateAsync(id);

            if (producto == null)
            {
                return new ResultadoServicio<ProductoDto>
                {
                    Estado = EstadoResultado.NoEncontrado,
                    Mensaje = "Producto no encontrado."
                };
            }

            // Verificar que el codigo no pertenezca a otro producto
            var productoConCodigo = await _productoRepository
                .FirstOrDefaultAsync(p => p.Codigo == productoDto.Codigo);

            if (productoConCodigo != null && productoConCodigo.Id != id)
            {
                return new ResultadoServicio<ProductoDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = "El codigo pertenece a otro producto."
                };
            }

            // Actualizar los campos permitidos
            _mapper.Map(productoDto, producto);

            try
            {
                await _productoRepository.SaveAsync();
            }
            catch (DuplicateKeyException)
            {
                return new ResultadoServicio<ProductoDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = "El codigo pertenece a otro producto."
                };
            }

            var productoActualizado = await _productoRepository
                .GetByIdAsync(id, p => p.Lotes);

            return new ResultadoServicio<ProductoDto>
            {
                Estado = EstadoResultado.Exito,
                Data = _mapper.Map<ProductoDto>(productoActualizado!)
            };
        }
        //PATCH
        public async Task<ResultadoServicio<bool>> CambiarEstadoProductoAsync(int id, CambiarEstadoProductoDto estadoDto)
        {
            var validacion = await _estadoValidator.ValidateAsync(estadoDto);
            if (!validacion.IsValid)
            {
                return new ResultadoServicio<bool>
                {
                    Estado = EstadoResultado.Validacion,
                    Mensaje = "Error de validacion.",
                    Errores = validacion.Errors.Select(error => new ErrorCampo
                    {
                        Campo = error.PropertyName,
                        Mensaje = error.ErrorMessage
                    }).ToList()
                };
            }

            var producto = await _productoRepository
                .GetForUpdateAsync(id);

            if (producto == null)
            {
                return new ResultadoServicio<bool>
                {
                    Estado = EstadoResultado.NoEncontrado,
                    Mensaje = "Producto no encontrado."
                };
            }

            producto.Activo = estadoDto.Activo!.Value;

            await _productoRepository.SaveAsync();

            return new ResultadoServicio<bool>
            {
                Estado = EstadoResultado.Exito,
                Data = producto.Activo == true
            };
        }


    }
}
