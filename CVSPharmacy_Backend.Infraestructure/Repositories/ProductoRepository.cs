using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Infraestructure.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly FarmaciaDbContext _farmaciaDbContext;

        public ProductoRepository(FarmaciaDbContext farmaciaDbContext)
        {
            _farmaciaDbContext = farmaciaDbContext;
        }

        public async Task<IEnumerable<Producto>> GetAllProductosAsync()
        {
            var productos = await _farmaciaDbContext.Productos
                .Include(p => p.Lotes)
                .AsNoTracking()
                .ToListAsync();

            return productos;
        }

        public async Task<Producto?> GetProductoByIdAsync(int id)
        {
            var producto = await _farmaciaDbContext.Productos
                .Include(p => p.Lotes)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return producto;
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
        
        public async Task InsertProducto(Producto producto)
        {
            _farmaciaDbContext.Productos.Add(producto);
            await _farmaciaDbContext.SaveChangesAsync();
        }

        public async Task UpdateProducto(Producto producto)
        {
            _farmaciaDbContext.Productos.Update(producto);
            await _farmaciaDbContext.SaveChangesAsync();
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
