using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IRecepcionRepository : IBaseRepository<Recepcion>
{
    Task<IEnumerable<Recepcion>> GetPorOrdenAsync(int ordenId);
    Task<List<Lote>> GetLotesPorOrdenAsync(int ordenId);
    Task<bool> ExisteLoteAsync(int productoId, string numeroLote);
}
