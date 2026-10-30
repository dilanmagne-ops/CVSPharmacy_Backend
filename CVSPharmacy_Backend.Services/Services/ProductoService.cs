using CVSPharmacy_Backend.Services.Interfaces;
using AutoMapper;
using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
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

        public async Task<ProductoDto> GetProductoByIdAsync(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id, p => p.Lotes);
            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.",HttpStatusCode.NotFound);
            }

            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<IEnumerable<ProductoDto>> GetProductosStockCriticoAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var productos = await _productoRepository.GetAllAsync(p =>
                (p.Lotes.Where(l => l.FechaVencimiento >= hoy && l.CantidadDisponible > 0)
                    .Sum(l => (int?)l.CantidadDisponible) ?? 0) <= p.StockMinimo, p => p.Lotes);
            return _mapper.Map<IEnumerable<ProductoDto>>(productos);
        }
        private async Task ValidarProductoAsync(GuardarProductoDto productoDto)
        {
            if (productoDto is null) throw new BusinessException("El cuerpo es obligatorio.");
            var resultado = await _validator.ValidateAsync(productoDto);

            if (!resultado.IsValid)
            {
                throw new BusinessException(
                    "Error de validacion.",
                    HttpStatusCode.BadRequest,
                    resultado.Errors.Select(error => new{field = error.PropertyName,error = error.ErrorMessage}).ToList());
            }
        }
        public async Task<ProductoDto> CrearProductoAsync(GuardarProductoDto productoDto)
        {
            // Validar los datos
            await ValidarProductoAsync(productoDto);
            // Verificar codigo duplicado
            var productoExistente = await _productoRepository.FirstOrDefaultAsync(p => p.Codigo == productoDto.Codigo);
            if (productoExistente != null)
            {
                throw new BusinessException("El codigo del producto ya existe.",HttpStatusCode.Conflict);
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
                throw new BusinessException("El codigo del producto ya existe.", HttpStatusCode.Conflict);
            }

            // Consultar el producto recien registrado
            var productoGuardado = await _productoRepository.GetByIdAsync(producto.Id, p => p.Lotes);
            var resultadoDto = _mapper.Map<ProductoDto>(productoGuardado ?? producto);
            return resultadoDto;
        }
        //PUT
        public async Task<ProductoDto> ActualizarProductoAsync(int id, GuardarProductoDto productoDto)
        {
            // Validar los datos
            await ValidarProductoAsync(productoDto);

            // Buscar producto existente
            var producto = await _productoRepository
                .GetForUpdateAsync(id);

            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.", HttpStatusCode.NotFound);
            }

            // Verificar que el codigo no pertenezca a otro producto
            var productoConCodigo = await _productoRepository
                .FirstOrDefaultAsync(p => p.Codigo == productoDto.Codigo);

            if (productoConCodigo != null && productoConCodigo.Id != id)
            {
                throw new BusinessException("El codigo pertenece a otro producto.", HttpStatusCode.Conflict);
            }
                                                                                                                                                                                                                                            
            // Actualizar los campos permitidos
            _mapper.Map(productoDto, producto);

            try
            {
                await _productoRepository.SaveAsync();
            }
            catch (DuplicateKeyException)
            {
                throw new BusinessException("El codigo pertenece a otro producto.", HttpStatusCode.Conflict);
            }

            var productoActualizado = await _productoRepository
                .GetByIdAsync(id, p => p.Lotes);

            return _mapper.Map<ProductoDto>(productoActualizado!);
        }
        //PATCH
        public async Task<bool> CambiarEstadoProductoAsync(int id, CambiarEstadoProductoDto estadoDto)
        {
            var validacion = await _estadoValidator.ValidateAsync(estadoDto);
            if (!validacion.IsValid)
            {
                throw new BusinessException("Error de validacion.",HttpStatusCode.BadRequest,
                    validacion.Errors.Select(error => new
                    {
                        field = error.PropertyName,
                        error = error.ErrorMessage
                    }).ToList());
            }

            var producto = await _productoRepository
                .GetForUpdateAsync(id);

            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.", HttpStatusCode.NotFound);
            }

            producto.Activo = estadoDto.Activo!.Value;

            await _productoRepository.SaveAsync();

            return producto.Activo == true;
        }


    }
}
