using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Auth;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infrastructure.Mappings;

public sealed class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<Usuario, UsuarioAutenticadoDto>()
            .ForCtorParam(nameof(UsuarioAutenticadoDto.Rol), opt => opt.MapFrom(u => u.Rol.Nombre));
    }
}
