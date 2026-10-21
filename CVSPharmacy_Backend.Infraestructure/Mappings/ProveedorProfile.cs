using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infraestructure.Mappings;

public sealed class ProveedorProfile : Profile
{
    public ProveedorProfile()
    {
        CreateMap<Proveedor, ProveedorDto>()
            .ForMember(destination => destination.Activo,
                options => options.MapFrom(source => source.Activo ?? false));

        CreateMap<GuardarProveedorDto, Proveedor>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Activo, options => options.Ignore())
            .ForMember(destination => destination.CreadoEn, options => options.Ignore())
            .ForMember(destination => destination.OrdenesCompras, options => options.Ignore())
            .ForMember(destination => destination.Nit,
                options => options.MapFrom(source => source.Nit.Trim()))
            .ForMember(destination => destination.RazonSocial,
                options => options.MapFrom(source => source.RazonSocial.Trim()))
            .ForMember(destination => destination.Contacto,
                options => options.MapFrom(source => CleanOptional(source.Contacto)))
            .ForMember(destination => destination.Telefono,
                options => options.MapFrom(source => CleanOptional(source.Telefono)))
            .ForMember(destination => destination.Email,
                options => options.MapFrom(source => CleanOptional(source.Email)))
            .ForMember(destination => destination.Direccion,
                options => options.MapFrom(source => CleanOptional(source.Direccion)));
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
