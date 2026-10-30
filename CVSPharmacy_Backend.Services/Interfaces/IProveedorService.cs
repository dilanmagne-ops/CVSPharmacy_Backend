using CVSPharmacy_Backend.Core.Dtos.Proveedor;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IProveedorService
{
    Task<IReadOnlyList<ProveedorDto>> GetAllAsync();
    Task<ProveedorDto?> GetByIdAsync(int id);
    Task<ProveedorDto> CreateAsync(GuardarProveedorDto proveedorDto);
    Task<ProveedorDto?> UpdateAsync(int id, GuardarProveedorDto proveedorDto);
    Task<ProveedorDto?> ChangeStatusAsync(int id, CambiarEstadoProveedorDto estadoDto);
}
