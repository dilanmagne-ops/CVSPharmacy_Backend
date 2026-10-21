using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;



namespace CVSPharmacy_Backend.Infraestructure.Repositories
{
    public class ProveedorRepository: IProveedorRepository
    {
        private readonly FarmaciaDbContext _dbContext;

        public ProveedorRepository(FarmaciaDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(Proveedor proveedor)
        {
            await _dbContext.Proveedores.AddAsync(proveedor);
        }

        public  async Task<IReadOnlyList<Proveedor>> GetAllAsync()
        {
            return await _dbContext.Proveedores
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Proveedor?> GetByIdAsync(int id)
        {
            return await _dbContext.Proveedores
             .AsNoTracking()
              .FirstOrDefaultAsync(proveedor => proveedor.Id == id);
        }

        public async Task<Proveedor?> GetForUpdateAsync(int id)
        {
            return await _dbContext.Proveedores
                .FirstOrDefaultAsync(proveedor => proveedor.Id == id);
        }

        public Task<bool> NitExistsAsync(string nit, int? excludeId = null)
        {
            return _dbContext.Proveedores
                .AnyAsync(proveedor => proveedor.Nit == nit && (!excludeId.HasValue || proveedor.Id != excludeId.Value));
        }

        public Task SaveAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

    }
}
