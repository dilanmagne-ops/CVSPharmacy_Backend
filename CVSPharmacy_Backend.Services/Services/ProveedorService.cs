using CVSPharmacy_Backend.Services.Interfaces;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Core.Results;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
using FluentValidation.Results;

namespace CVSPharmacy_Backend.Services.Services;

public sealed class ProveedorService : IProveedorService
{
    private readonly IBaseRepository<Proveedor> _repository;
    private readonly IMapper _mapper;
    private readonly GuardarProveedorDtoValidator _validator;
    private readonly CambiarEstadoProveedorDtoValidator _estadoValidator;

    public ProveedorService(IBaseRepository<Proveedor> repository, IMapper mapper,
        GuardarProveedorDtoValidator validator, CambiarEstadoProveedorDtoValidator estadoValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _estadoValidator = estadoValidator;
    }

    public async Task<IReadOnlyList<ProveedorDto>> GetAllAsync() =>
        _mapper.Map<IReadOnlyList<ProveedorDto>>(await _repository.GetAllAsync());

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        var proveedor = await _repository.GetByIdAsync(id);
        return proveedor is null ? null : _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ResultadoServicio<ProveedorDto>> CreateAsync(GuardarProveedorDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid) return Invalid(validation);
        if (await _repository.AnyAsync(p => p.Nit == dto.Nit.Trim())) return Duplicate();

        var proveedor = _mapper.Map<Proveedor>(dto);
        proveedor.Activo = true;
        await _repository.AddAsync(proveedor);
        return await SaveAsync(proveedor);
    }

    public async Task<ResultadoServicio<ProveedorDto>> UpdateAsync(int id, GuardarProveedorDto dto)
    {
        var validation = await _validator.ValidateAsync(dto);
        if (!validation.IsValid) return Invalid(validation);
        var proveedor = await _repository.GetForUpdateAsync(id);
        if (proveedor is null) return Missing();
        if (await _repository.AnyAsync(p => p.Nit == dto.Nit.Trim() && p.Id != id)) return Duplicate();

        _mapper.Map(dto, proveedor);
        return await SaveAsync(proveedor);
    }

    public async Task<ResultadoServicio<ProveedorDto>> ChangeStatusAsync(int id, CambiarEstadoProveedorDto dto)
    {
        var validation = await _estadoValidator.ValidateAsync(dto);
        if (!validation.IsValid) return Invalid(validation);
        var proveedor = await _repository.GetForUpdateAsync(id);
        if (proveedor is null) return Missing();
        proveedor.Activo = dto.Activo!.Value;
        return await SaveAsync(proveedor);
    }

    private async Task<ResultadoServicio<ProveedorDto>> SaveAsync(Proveedor proveedor)
    {
        try { await _repository.SaveAsync(); }
        catch (DuplicateKeyException) { return Duplicate(); }
        return new() { Data = _mapper.Map<ProveedorDto>(proveedor) };
    }

    private static ResultadoServicio<ProveedorDto> Invalid(ValidationResult validation) => new()
    {
        Estado = EstadoResultado.Validacion,
        Mensaje = "Error de validación.",
        Errores = validation.Errors.Select(e => new ErrorCampo
        { Campo = e.PropertyName, Mensaje = e.ErrorMessage }).ToList()
    };

    private static ResultadoServicio<ProveedorDto> Missing() => new()
    { Estado = EstadoResultado.NoEncontrado, Mensaje = "Proveedor no encontrado." };

    private static ResultadoServicio<ProveedorDto> Duplicate() => new()
    { Estado = EstadoResultado.Conflicto, Mensaje = "Ya existe un proveedor con ese NIT." };
}
