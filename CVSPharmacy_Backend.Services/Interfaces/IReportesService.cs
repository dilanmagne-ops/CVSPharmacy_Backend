using CVSPharmacy_Backend.Core.CustomEntities.Reporteria;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IReportesService
{
    Task<IEnumerable<RankingProductoProveedor>> GetRankingProductosPorProveedorAsync();
}
