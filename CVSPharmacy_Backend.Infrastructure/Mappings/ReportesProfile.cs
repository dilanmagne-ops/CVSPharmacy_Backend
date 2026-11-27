using AutoMapper;
using CVSPharmacy_Backend.Core.CustomEntities.Reporteria;
using CVSPharmacy_Backend.Core.Dtos.Reporteria;

namespace CVSPharmacy_Backend.Infrastructure.Mappings;

public sealed class ReportesProfile : Profile
{
    public ReportesProfile() => CreateMap<RankingProductoProveedor, RankingProductoProveedorDto>();
}
