using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IProveedorRepository : IBaseRepository<Proveedor>
{
    Task<Proveedor?> GetForUpdateAsync(int id);
    Task<bool> NitExistsAsync(string nit, int? excludeId = null);
}
