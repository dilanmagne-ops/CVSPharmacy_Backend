using AutoMapper;
using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Producto;
using CVSPharmacy_Backend.Core.Dtos.Producto;

namespace CVSPharmacy_Backend.Services.Service
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        private readonly GuardarProductoDtoValidator _validator;
        private readonly CambiarEstadoProductoDtoValidator _estadoValidator;

        public ProductoService(IProductoRepository productoRepository, IMapper mapper, GuardarProductoDtoValidator validator, CambiarEstadoProductoDtoValidator estadoValidator)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
            _validator = validator;
            _estadoValidator = estadoValidator;
        }
        public async Task<IEnumerable<ProductoDto>> GetAllProductosAsync(string? q, bool? active)
        {
            var productos = await _productoRepository.GetAll();
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
            var producto = await _productoRepository.GetById(id);
            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.",HttpStatusCode.NotFound);
            }

            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<IEnumerable<ProductoDto>> GetProductosStockCriticoAsync()
        {
            var productos = await _productoRepository.GetProductosStockCriticoAsync();
            return _mapper.Map<IEnumerable<ProductoDto>>(productos);
        }
        private async Task ValidarProductoAsync(GuardarProductoDto productoDto)
        {
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
            var productoExistente = await _productoRepository.GetProductoByCodigoAsync(productoDto.Codigo);
            if (productoExistente != null)
            {
                throw new BusinessException("El codigo del producto ya existe.",HttpStatusCode.Conflict);
            }
            // Convertir DTO a entidad
            var producto = _mapper.Map<Producto>(productoDto);
            producto.Activo = true;
            try
            {
                await _productoRepository.Add(producto);
            }
            catch (CodigoProductoDuplicadoException)
            {
                throw new BusinessException("El codigo del producto ya existe.", HttpStatusCode.Conflict);
            }

            // Consultar el producto recien registrado
            var productoGuardado = await _productoRepository.GetById(producto.Id);
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
                .GetProductoParaActualizarAsync(id);

            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.", HttpStatusCode.NotFound);
            }

            // Verificar que el codigo no pertenezca a otro producto
            var productoConCodigo = await _productoRepository
                .GetProductoByCodigoAsync(productoDto.Codigo);

            if (productoConCodigo != null && productoConCodigo.Id != id)
            {
                throw new BusinessException("El codigo pertenece a otro producto.", HttpStatusCode.Conflict);
            }
                                                                                                                                                                                                                                            
            // Actualizar los campos permitidos
            _mapper.Map(productoDto, producto);

            try
            {
                await _productoRepository.Update(producto);
            }
            catch (CodigoProductoDuplicadoException)
            {
                throw new BusinessException("El codigo pertenece a otro producto.", HttpStatusCode.Conflict);
            }

            var productoActualizado = await _productoRepository
                .GetById(id);

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
                .GetProductoParaActualizarAsync(id);

            if (producto == null)
            {
                throw new BusinessException("Producto no encontrado.", HttpStatusCode.NotFound);
            }

            producto.Activo = estadoDto.Activo!.Value;

            await _productoRepository.Update(producto);

            return producto.Activo == true;
        }


    }
}
