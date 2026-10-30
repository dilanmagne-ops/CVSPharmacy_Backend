using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using CVSPharmacy_Backend.Core.Exceptions;
using MySqlConnector;

namespace CVSPharmacy_Backend.Infraestructure.Repositories
{
    public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
    {
        private readonly FarmaciaDbContext _farmaciaDbContext;

        public ProductoRepository(FarmaciaDbContext farmaciaDbContext) : base(farmaciaDbContext)
        {
            _farmaciaDbContext = farmaciaDbContext;
        }

        public override async Task<IEnumerable<Producto>> GetAll()
        {
            return await _entities
                .Include(p => p.Lotes)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Producto?> GetById(int id)
        {
            return await _entities
                .Include(p => p.Lotes)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Producto?> GetProductoParaActualizarAsync(int id)
        {
            var producto = await _farmaciaDbContext.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            return producto;
        }
        public async Task<Producto?> GetProductoByCodigoAsync(string codigo)
        {
            var producto = await _farmaciaDbContext.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Codigo == codigo);

            return producto;
        }

        public override async Task Add(Producto producto)
        {
            try
            {
                await base.Add(producto);
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                throw new CodigoProductoDuplicadoException(ex);
            }
        }

        public override async Task Update(Producto producto)
        {
            try
            {
                await base.Update(producto);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is MySqlException { Number: 1062 })
            {
                throw new CodigoProductoDuplicadoException(ex);
            }
        }

        public async Task<IEnumerable<Producto>> GetProductosStockCriticoAsync()
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

            var productos = await _farmaciaDbContext.Productos
                .Include(p => p.Lotes)
                .AsNoTracking()
                .Where(p =>
                    (p.Lotes
                        .Where(l => l.FechaVencimiento >= hoy
                                 && l.CantidadDisponible > 0)
                        .Sum(l => (int?)l.CantidadDisponible) ?? 0)
                    <= p.StockMinimo)
                .ToListAsync();

            return productos;
        }
    }
}
