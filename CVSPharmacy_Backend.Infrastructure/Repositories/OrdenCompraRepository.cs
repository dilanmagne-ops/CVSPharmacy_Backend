using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CVSPharmacy_Backend.Core.Exceptions;
namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class OrdenCompraRepository : BaseRepository<OrdenesCompra>, IOrdenCompraRepository
{
    public OrdenCompraRepository(FarmaciaDbContext context)
        : base(context)
    {
    }

    public override async Task<IEnumerable<OrdenesCompra>> GetAll()
    {
        return await _entities
            .AsNoTracking()
            .Include(orden => orden.Proveedor)
            .Include(orden => orden.DetalleOrdenCompras)
                .ThenInclude(detalle => detalle.Producto)
            .ToListAsync();
    }

    public override async Task<OrdenesCompra?> GetById(int id)
    {
        return await _entities
            .AsNoTracking()
            .Include(orden => orden.Proveedor)
            .Include(orden => orden.DetalleOrdenCompras)
                .ThenInclude(detalle => detalle.Producto)
            .FirstOrDefaultAsync(orden => orden.Id == id);
    }

    public async Task<OrdenesCompra?> GetOrdenParaActualizarAsync(int id)
    {
        if (_context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "La lectura para modificar una orden requiere una transaccion.");
        }

        // La fila queda bloqueada hasta terminar la transaccion de CU-05.
        // No se compone Include sobre el SQL que termina en FOR UPDATE.
        var filas = await _entities.FromSqlInterpolated(
            $"SELECT * FROM ordenes_compra WHERE id = {id} FOR UPDATE")
            .ToListAsync();
        var orden = filas.SingleOrDefault();
        if (orden is not null)
        {
            await _context.Entry(orden)
                .Collection(o => o.DetalleOrdenCompras).LoadAsync();
        }
        return orden;
    }

    public async Task<bool> TieneRecepcionesAsync(int ordenId)
    {
        return await _context.Recepciones
            .AnyAsync(recepcion => recepcion.OrdenId == ordenId);
    }

    public void ReemplazarDetalles(
        OrdenesCompra orden,
        IEnumerable<DetalleOrdenCompra> nuevosDetalles)
    {
        var nuevos = nuevosDetalles.ToList();
        var idsProductos = nuevos.Select(detalle => detalle.ProductoId).ToHashSet();
        var retirados = orden.DetalleOrdenCompras
            .Where(detalle => !idsProductos.Contains(detalle.ProductoId)).ToList();

        _context.DetalleOrdenCompras.RemoveRange(retirados);
        foreach (var retirado in retirados)
        {
            orden.DetalleOrdenCompras.Remove(retirado);
        }

        // Conservar los Ids de productos que siguen en la orden evita insertar
        // el mismo par orden/producto antes de eliminar su detalle anterior.
        foreach (var nuevo in nuevos)
        {
            var existente = orden.DetalleOrdenCompras
                .SingleOrDefault(detalle => detalle.ProductoId == nuevo.ProductoId);
            if (existente is null)
            {
                orden.DetalleOrdenCompras.Add(nuevo);
            }
            else
            {
                existente.CantidadSolicitada = nuevo.CantidadSolicitada;
                existente.PrecioUnitario = nuevo.PrecioUnitario;
            }
        }
    }

    public async Task<List<string>> GetNumerosOrdenAsync(string prefijo)
    {
        return await _entities
            .AsNoTracking()
            .Where(orden => orden.Numero.StartsWith(prefijo))
            .Select(orden => orden.Numero)
            .ToListAsync();
    }
    public override Task Delete(int id)
    {
        throw new BusinessException(
            "Las ordenes de compra no se eliminan; utilice la cancelacion.",
            HttpStatusCode.Conflict);
    }
}
