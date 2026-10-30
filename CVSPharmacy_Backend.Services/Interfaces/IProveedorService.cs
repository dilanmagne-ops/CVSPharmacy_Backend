using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Results;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IProveedorService
{
    Task<IReadOnlyList<ProveedorDto>> GetAllAsync();
    Task<ProveedorDto?> GetByIdAsync(int id);
    Task<ResultadoServicio<ProveedorDto>> CreateAsync(GuardarProveedorDto proveedorDto);
    Task<ResultadoServicio<ProveedorDto>> UpdateAsync(int id, GuardarProveedorDto proveedorDto);
    Task<ResultadoServicio<ProveedorDto>> ChangeStatusAsync(int id, CambiarEstadoProveedorDto estadoDto);
}
