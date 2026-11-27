using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infrastructure.Mappings;

public class OrdenCompraProfile : Profile
{
    public OrdenCompraProfile()
    {
        CreateMap<DetalleOrdenCompra, DetalleOrdenCompraDto>()
            .ForMember(dest => dest.ProductoCodigo,
                opt => opt.MapFrom(src => src.Producto.Codigo))
            .ForMember(dest => dest.ProductoNombre,
                opt => opt.MapFrom(src => src.Producto.Nombre));

        CreateMap<OrdenesCompra, OrdenCompraDto>()
            .ForMember(dest => dest.ProveedorRazonSocial,
                opt => opt.MapFrom(src => src.Proveedor.RazonSocial))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.DetalleOrdenCompras))
            .ForMember(dest => dest.Total,
                opt => opt.MapFrom(src =>
                    src.DetalleOrdenCompras.Sum(item =>
                        item.CantidadSolicitada * item.PrecioUnitario)));

        CreateMap<ItemOrdenCompraDto, DetalleOrdenCompra>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrdenId, opt => opt.Ignore())
            .ForMember(dest => dest.Orden, opt => opt.Ignore())
            .ForMember(dest => dest.Producto, opt => opt.Ignore())
            .ForMember(dest => dest.Lotes, opt => opt.Ignore());
    }
}
