using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Proveedor;

namespace CVSPharmacy_Backend.Services;

public sealed class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly GuardarProveedorDtoValidator _validator;
    private readonly CambiarEstadoProveedorDtoValidator _estadoValidator;

    public ProveedorService(IProveedorRepository proveedorRepository, IMapper mapper,
        GuardarProveedorDtoValidator validator, CambiarEstadoProveedorDtoValidator estadoValidator)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _validator = validator;
        _estadoValidator = estadoValidator;
    }

    public async Task<IReadOnlyList<ProveedorDto>> GetAllAsync(string? q = null, bool? active = null)
    {
        var proveedores = await _proveedorRepository.GetAll();
        // Mismo enfoque de filtros en memoria que CU-03.
        if (!string.IsNullOrWhiteSpace(q))
        {
            var busqueda = q.Trim();
            proveedores = proveedores.Where(proveedor =>
                proveedor.Nit.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                proveedor.RazonSocial.Contains(busqueda, StringComparison.OrdinalIgnoreCase));
        }
        if (active.HasValue)
        {
            proveedores = proveedores.Where(proveedor => proveedor.Activo == active.Value);
        }
        return _mapper.Map<IReadOnlyList<ProveedorDto>>(proveedores);
    }

    public async Task<ProveedorDto> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID del proveedor debe ser mayor que cero.");
        }
        var proveedor = await _proveedorRepository.GetById(id);
        if (proveedor is null)
        {
            throw new BusinessException("Proveedor no encontrado.", HttpStatusCode.NotFound);
        }
        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto> CreateAsync(GuardarProveedorDto proveedorDto)
    {
        if (proveedorDto is null)
        {
            throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        }
        var resultado = await _validator.ValidateAsync(proveedorDto);
        if (!resultado.IsValid)
        {
            throw new BusinessException("Error de validacion.", HttpStatusCode.BadRequest,
                resultado.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }
        if (await _proveedorRepository.NitExistsAsync(proveedorDto.Nit.Trim()))
        {
            throw new BusinessException("El NIT ya pertenece a un proveedor registrado.", HttpStatusCode.Conflict);
        }
        var proveedor = _mapper.Map<Proveedor>(proveedorDto);
        proveedor.Activo = true;
        try
        {
            await _proveedorRepository.Add(proveedor);
        }
        catch (NitProveedorDuplicadoException)
        {
            throw new BusinessException("El NIT ya pertenece a un proveedor registrado.", HttpStatusCode.Conflict);
        }
        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto> UpdateAsync(int id, GuardarProveedorDto proveedorDto)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID del proveedor debe ser mayor que cero.");
        }
        if (proveedorDto is null)
        {
            throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        }
        var resultado = await _validator.ValidateAsync(proveedorDto);
        if (!resultado.IsValid)
        {
            throw new BusinessException("Error de validacion.", HttpStatusCode.BadRequest,
                resultado.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }
        var proveedor = await _proveedorRepository.GetForUpdateAsync(id);
        if (proveedor is null)
        {
            throw new BusinessException("Proveedor no encontrado.", HttpStatusCode.NotFound);
        }
        if (await _proveedorRepository.NitExistsAsync(proveedorDto.Nit.Trim(), id))
        {
            throw new BusinessException("El NIT ya pertenece a un proveedor registrado.", HttpStatusCode.Conflict);
        }
        _mapper.Map(proveedorDto, proveedor);
        try
        {
            await _proveedorRepository.Update(proveedor);
        }
        catch (NitProveedorDuplicadoException)
        {
            throw new BusinessException("El NIT ya pertenece a un proveedor registrado.", HttpStatusCode.Conflict);
        }
        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto> ChangeStatusAsync(int id, CambiarEstadoProveedorDto estadoDto)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID del proveedor debe ser mayor que cero.");
        }
        if (estadoDto is null)
        {
            throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        }
        var validacion = await _estadoValidator.ValidateAsync(estadoDto);
        if (!validacion.IsValid)
        {
            throw new BusinessException("Error de validacion.", HttpStatusCode.BadRequest,
                validacion.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }
        var proveedor = await _proveedorRepository.GetForUpdateAsync(id);
        if (proveedor is null)
        {
            throw new BusinessException("Proveedor no encontrado.", HttpStatusCode.NotFound);
        }
        proveedor.Activo = estadoDto.Activo!.Value;
        // No se modifican las órdenes ni sus detalles al cambiar el estado (RN-07/RN-09).
        await _proveedorRepository.Update(proveedor);
        return _mapper.Map<ProveedorDto>(proveedor);
    }
}
