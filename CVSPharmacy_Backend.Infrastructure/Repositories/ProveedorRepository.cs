using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class ProveedorRepository : BaseRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(FarmaciaDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Proveedor>> GetAll()
    {
        return await _entities.AsNoTracking().ToListAsync();
    }

    public override async Task<Proveedor?> GetById(int id)
    {
        return await _entities.AsNoTracking()
            .FirstOrDefaultAsync(proveedor => proveedor.Id == id);
    }

    public Task<Proveedor?> GetForUpdateAsync(int id)
    {
        return _entities.FirstOrDefaultAsync(proveedor => proveedor.Id == id);
    }

    public Task<bool> NitExistsAsync(string nit, int? excludeId = null)
    {
        return _entities.AnyAsync(proveedor => proveedor.Nit == nit &&
            (!excludeId.HasValue || proveedor.Id != excludeId.Value));
    }

    public override Task Delete(int id)
    {
        // RN-07: conservar el registro y su historial; la baja se realiza por estado.
        throw new BusinessException(
            "Los proveedores no se eliminan; utilice el cambio de estado.",
            HttpStatusCode.Conflict);
    }
}
