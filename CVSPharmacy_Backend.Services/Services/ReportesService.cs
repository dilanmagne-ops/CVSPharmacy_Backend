using CVSPharmacy_Backend.Core.CustomEntities.Reporteria;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public sealed class ReportesService(IUnitOfWork unitOfWork) : IReportesService
{
    public Task<IEnumerable<RankingProductoProveedor>> GetRankingProductosPorProveedorAsync() =>
        unitOfWork.ReportesRepository.GetRankingProductosPorProveedorAsync();
}
