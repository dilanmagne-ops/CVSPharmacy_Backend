using AutoMapper;
using System.Net;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
namespace CVSPharmacy_Backend.Services.Services;

public sealed class ProveedorService(IBaseRepository<Proveedor> repository, IMapper mapper,
    GuardarProveedorDtoValidator validator, CambiarEstadoProveedorDtoValidator estadoValidator)
    : IProveedorService
{
    public async Task<IReadOnlyList<ProveedorDto>> GetAllAsync() =>
        mapper.Map<IReadOnlyList<ProveedorDto>>(await repository.GetAllAsync());
    public async Task<ProveedorDto?> GetByIdAsync(int id) =>
        mapper.Map<ProveedorDto?>(await repository.GetByIdAsync(id));
    public async Task<ProveedorDto> CreateAsync(GuardarProveedorDto dto)
    {
        await ValidacionEntrada.ValidarAsync(validator, dto);
        if (await repository.AnyAsync(p => p.Nit == dto.Nit.Trim())) throw Duplicado();
        var proveedor = mapper.Map<Proveedor>(dto); proveedor.Activo = true;
        await repository.AddAsync(proveedor);
        return await GuardarAsync(proveedor);
    }
    public async Task<ProveedorDto?> UpdateAsync(int id, GuardarProveedorDto dto)
    {
        await ValidacionEntrada.ValidarAsync(validator, dto);
        var proveedor = await repository.GetForUpdateAsync(id);
        if (proveedor is null) throw new BusinessException("Proveedor no encontrado.", HttpStatusCode.NotFound);
        if (await repository.AnyAsync(p => p.Nit == dto.Nit.Trim() && p.Id != id)) throw Duplicado();
        mapper.Map(dto, proveedor);
        return await GuardarAsync(proveedor);
    }
    public async Task<ProveedorDto?> ChangeStatusAsync(int id, CambiarEstadoProveedorDto dto)
    {
        await ValidacionEntrada.ValidarAsync(estadoValidator, dto);
        var proveedor = await repository.GetForUpdateAsync(id);
        if (proveedor is null) throw new BusinessException("Proveedor no encontrado.", HttpStatusCode.NotFound);
        proveedor.Activo = dto.Activo!.Value;
        return await GuardarAsync(proveedor);
    }
    private async Task<ProveedorDto> GuardarAsync(Proveedor proveedor)
    {
        try { await repository.SaveAsync(); }
        catch (DuplicateKeyException) { throw Duplicado(); }
        return mapper.Map<ProveedorDto>(proveedor);
    }
    private static BusinessException Duplicado() => new("Ya existe un proveedor con ese NIT.", HttpStatusCode.Conflict);
}
