using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IProveedorService
{
    Task<IReadOnlyList<Proveedor>> GetAllAsync(ProveedorQueryFilters filters);
    Task<Proveedor> GetByIdAsync(int id);
    Task<Proveedor> CreateAsync(Proveedor proveedor);
    Task<Proveedor> UpdateAsync(int id, Proveedor cambios);
    Task<Proveedor> ChangeStatusAsync(int id, bool activo);
}
