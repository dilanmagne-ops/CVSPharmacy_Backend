using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infraestructure.Mappings;

public class RecepcionProfile : Profile
{
    public RecepcionProfile()
    {
        CreateMap<Recepcion, RecepcionDto>();
        CreateMap<Lote, LoteRecepcionDto>();
    }
}
