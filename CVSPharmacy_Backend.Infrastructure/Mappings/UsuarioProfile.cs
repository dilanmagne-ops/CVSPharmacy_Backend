using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Usuario;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infrastructure.Mappings;

public sealed class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioDto>().ForCtorParam(nameof(UsuarioDto.Rol),
            opt => opt.MapFrom(u => u.Rol.Nombre));
    }
}
