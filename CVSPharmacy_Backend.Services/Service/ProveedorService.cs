using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;

namespace CVSPharmacy_Backend.Services;

public sealed class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;

    public ProveedorService(IProveedorRepository proveedorRepository, IMapper mapper)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProveedorDto>> GetAllAsync()
    {
        var proveedores = await _proveedorRepository.GetAllAsync();
        return _mapper.Map<IReadOnlyList<ProveedorDto>>(proveedores);
    }

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        var proveedor = await _proveedorRepository.GetByIdAsync(id);
        return proveedor is null ? null : _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto> CreateAsync(GuardarProveedorDto proveedorDto)
    {
        var nit = proveedorDto.Nit.Trim();

        if (await _proveedorRepository.NitExistsAsync(nit))
            throw new InvalidOperationException("Ya existe un proveedor con ese NIT.");

        var proveedor = _mapper.Map<Proveedor>(proveedorDto);
        proveedor.Activo = true;

        await _proveedorRepository.AddAsync(proveedor);
        await _proveedorRepository.SaveAsync();
        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto?> UpdateAsync(int id, GuardarProveedorDto proveedorDto)
    {
        var proveedor = await _proveedorRepository.GetForUpdateAsync(id);
        if (proveedor is null)
            return null;

        var nit = proveedorDto.Nit.Trim();
        if (await _proveedorRepository.NitExistsAsync(nit, id))
            throw new InvalidOperationException("El NIT ya pertenece a otro proveedor.");

        _mapper.Map(proveedorDto, proveedor);

        await _proveedorRepository.SaveAsync();
        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto?> ChangeStatusAsync(int id, CambiarEstadoProveedorDto estadoDto)
    {
        var proveedor = await _proveedorRepository.GetForUpdateAsync(id);
        if (proveedor is null)
            return null;

        proveedor.Activo = estadoDto.Activo!.Value;
        await _proveedorRepository.SaveAsync();
        return _mapper.Map<ProveedorDto>(proveedor);
    }

}
