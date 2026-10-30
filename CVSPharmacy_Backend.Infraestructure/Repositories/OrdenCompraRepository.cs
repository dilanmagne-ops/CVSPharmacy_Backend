using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector; // usa el mismo using de MySqlException que tienes en ProductoRepository

namespace CVSPharmacy_Backend.Infraestructure.Repositories;

public class OrdenCompraRepository : IOrdenCompraRepository
{
    private readonly FarmaciaDbContext _farmaciaDbContext;

    public OrdenCompraRepository(FarmaciaDbContext farmaciaDbContext)
    {
        _farmaciaDbContext = farmaciaDbContext;
    }

    public async Task<IEnumerable<OrdenesCompra>> GetAllOrdenesAsync(
        string? estado, int? proveedorId, DateTime? desde, DateTime? hasta)
    {
        var query = _farmaciaDbContext.OrdenesCompras
            .AsNoTracking()
            .Include(o => o.Proveedor)
            .Include(o => o.DetalleOrdenCompras)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(o => o.Estado == estado);

        if (proveedorId.HasValue)
            query = query.Where(o => o.ProveedorId == proveedorId.Value);

        if (desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= desde.Value.Date);

        if (hasta.HasValue)
            query = query.Where(o => o.FechaCreacion < hasta.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<OrdenesCompra?> GetOrdenByIdAsync(int id)
    {
        return await _farmaciaDbContext.OrdenesCompras
            .AsNoTracking()
            .Include(o => o.Proveedor)
            .Include(o => o.DetalleOrdenCompras)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrdenesCompra?> GetOrdenParaActualizarAsync(int id)
    {
        return await _farmaciaDbContext.OrdenesCompras
            .Include(o => o.DetalleOrdenCompras)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Proveedor?> GetProveedorAsync(int id)
    {
        return await _farmaciaDbContext.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Producto>> GetProductosAsync(IEnumerable<int> ids)
    {
        var lista = ids.ToList();
        return await _farmaciaDbContext.Productos
            .AsNoTracking()
            .Where(p => lista.Contains(p.Id))
            .ToListAsync();
    }

    // Formato OC-2026-0001: el correlativo reinicia cada año
    public async Task<string> GenerarNumeroAsync()
    {
        var prefijo = $"OC-{DateTime.Now.Year}-";

        var ultimo = await _farmaciaDbContext.OrdenesCompras
            .AsNoTracking()
            .Where(o => o.Numero.StartsWith(prefijo))
            .OrderByDescending(o => o.Numero)
            .Select(o => o.Numero)
            .FirstOrDefaultAsync();

        var siguiente = 1;
        if (ultimo != null && int.TryParse(ultimo.Substring(prefijo.Length), out var actual))
            siguiente = actual + 1;

        return $"{prefijo}{siguiente:D4}";
    }

    public async Task<bool> TieneRecepcionesAsync(int ordenId)
    {
        return await _farmaciaDbContext.Recepciones
            .AsNoTracking()
            .AnyAsync(r => r.OrdenId == ordenId);
    }

    // Un solo SaveChanges guarda la orden y sus ítems en una transacción
    public async Task InsertOrdenAsync(OrdenesCompra orden)
    {
        _farmaciaDbContext.OrdenesCompras.Add(orden);
        try
        {
            await _farmaciaDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is MySqlException { Number: 1062 })
        {
            // Se limpia para poder reintentar con otro número
            _farmaciaDbContext.ChangeTracker.Clear();
            throw new NumeroOrdenDuplicadoException(ex);
        }
    }

    public void ReemplazarDetalles(OrdenesCompra orden, IEnumerable<DetalleOrdenCompra> nuevos)
    {
        _farmaciaDbContext.DetalleOrdenCompras.RemoveRange(orden.DetalleOrdenCompras);

        foreach (var detalle in nuevos)
            orden.DetalleOrdenCompras.Add(detalle);
    }

    public async Task SaveAsync()
    {
        await _farmaciaDbContext.SaveChangesAsync();
    }
}
