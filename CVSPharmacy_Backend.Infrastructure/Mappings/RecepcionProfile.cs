using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infrastructure.Mappings;

public class RecepcionProfile : Profile
{
    public RecepcionProfile()
    {
        CreateMap<GuardarRecepcionDto, Recepcion>(MemberList.Source)
            .ForMember(d => d.Lotes, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.NroRemision, o => o.MapFrom(s => s.NroRemision == null ? null : s.NroRemision.Trim()))
            .ForMember(d => d.Observaciones, o => o.MapFrom(s => s.Observaciones == null ? null : s.Observaciones.Trim()));
        CreateMap<LoteRecibidoDto, Lote>(MemberList.Source)
            .ForMember(d => d.NumeroLote, o => o.MapFrom(s => s.NumeroLote.Trim()))
            .ForMember(d => d.CantidadRecibida, o => o.MapFrom(s => s.Cantidad));
        CreateMap<Recepcion, RecepcionDto>();
        CreateMap<Lote, LoteRecepcionDto>();
    }
}
