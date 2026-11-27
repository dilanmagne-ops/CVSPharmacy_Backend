using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infrastructure.Repositories
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

        // Producto confirma los cambios mediante la unidad de trabajo.
        protected override Task GuardarSiCorrespondeAsync() => Task.CompletedTask;

        public async Task<List<Producto>> GetProductosAsync(
            IEnumerable<int> ids)
        {
            var idsProductos = ids.Distinct().ToList();

            return await _context.Productos
                .AsNoTracking()
                .Where(producto => idsProductos.Contains(producto.Id))
                .ToListAsync();
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
