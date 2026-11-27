using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class RecepcionRepository : BaseRepository<Recepcion>, IRecepcionRepository
{
    public RecepcionRepository(FarmaciaDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Recepcion>> GetAll()
    {
        return await _entities.AsNoTracking().Include(recepcion => recepcion.Lotes).ToListAsync();
    }

    public override async Task<Recepcion?> GetById(int id)
    {
        return await _entities.AsNoTracking().Include(recepcion => recepcion.Lotes)
            .FirstOrDefaultAsync(recepcion => recepcion.Id == id);
    }

    public async Task<IEnumerable<Recepcion>> GetPorOrdenAsync(int ordenId)
    {
        return await _entities.AsNoTracking().Include(recepcion => recepcion.Lotes)
            .Where(recepcion => recepcion.OrdenId == ordenId)
            .OrderBy(recepcion => recepcion.Id).ToListAsync();
    }

    public async Task<List<Lote>> GetLotesPorOrdenAsync(int ordenId)
    {
        return await _context.Lotes.AsNoTracking()
            .Where(lote => lote.DetalleOrden.OrdenId == ordenId).ToListAsync();
    }

    public Task<bool> ExisteLoteAsync(int productoId, string numeroLote)
    {
        // Utiliza la comparacion de MySQL y el mismo indice unico de producto/lote.
        return _context.Lotes.AnyAsync(lote =>
            lote.ProductoId == productoId && lote.NumeroLote == numeroLote);
    }

    public override Task Delete(int id)
    {
        throw new BusinessException("Las recepciones no se eliminan; forman parte del historial.",
            HttpStatusCode.Conflict);
    }
}
