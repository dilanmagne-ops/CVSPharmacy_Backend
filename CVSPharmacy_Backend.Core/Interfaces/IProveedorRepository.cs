using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IProveedorRepository
{
    Task<IReadOnlyList<Proveedor>> GetAllAsync();
    Task<Proveedor?> GetByIdAsync(int id);
    Task<Proveedor?> GetForUpdateAsync(int id);
    Task<bool> NitExistsAsync(string nit, int? excludeId = null);
    Task AddAsync(Proveedor proveedor);
    Task SaveAsync();
}
