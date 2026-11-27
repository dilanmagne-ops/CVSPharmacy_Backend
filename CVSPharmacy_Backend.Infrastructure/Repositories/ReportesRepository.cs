using CVSPharmacy_Backend.Core.CustomEntities.Reporteria;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Queries;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public sealed class ReportesRepository(IDapperContext dapper) : IReportesRepository
{
    public Task<IEnumerable<RankingProductoProveedor>> GetRankingProductosPorProveedorAsync() =>
        dapper.QueryAsync<RankingProductoProveedor>(Reportes.RankingProductosPorProveedorMySql);
}
