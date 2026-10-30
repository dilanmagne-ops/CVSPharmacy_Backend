using AutoMapper;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
namespace CVSPharmacy_Backend.Infraestructure.Mappings;
public sealed class RecepcionProfile : Profile
{
    public RecepcionProfile()
    {
        CreateMap<Recepcion, RecepcionDto>();
        CreateMap<Lote, LoteRecepcionDto>();
    }
}
