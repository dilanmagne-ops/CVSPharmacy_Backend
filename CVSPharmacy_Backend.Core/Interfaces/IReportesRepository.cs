using CVSPharmacy_Backend.Core.CustomEntities.Reporteria;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IReportesRepository
{
    Task<IEnumerable<RankingProductoProveedor>> GetRankingProductosPorProveedorAsync();
}
