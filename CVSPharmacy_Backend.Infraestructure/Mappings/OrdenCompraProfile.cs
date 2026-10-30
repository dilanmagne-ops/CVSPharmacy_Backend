using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infraestructure.Mappings;

public sealed class OrdenCompraProfile : Profile
{
    public OrdenCompraProfile()
    {
        CreateMap<DetalleOrdenCompra, DetalleOrdenCompraDto>()
            .ForMember(destination => destination.ProductoCodigo,
                options => options.MapFrom(source => source.Producto.Codigo))
            .ForMember(destination => destination.ProductoNombre,
                options => options.MapFrom(source => source.Producto.Nombre));

        CreateMap<OrdenesCompra, OrdenCompraDto>()
            .ForMember(destination => destination.ProveedorRazonSocial,
                options => options.MapFrom(source => source.Proveedor.RazonSocial))
            .ForMember(destination => destination.Items,
                options => options.MapFrom(source => source.DetalleOrdenCompras))
            .ForMember(destination => destination.Total,
                options => options.MapFrom(source =>
                    source.DetalleOrdenCompras.Sum(item => item.CantidadSolicitada * item.PrecioUnitario)));

        // De la petición a la entidad: el id, la orden, el producto y los lotes los maneja EF
        CreateMap<ItemOrdenCompraDto, DetalleOrdenCompra>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.OrdenId, options => options.Ignore())
            .ForMember(destination => destination.Orden, options => options.Ignore())
            .ForMember(destination => destination.Producto, options => options.Ignore())
            .ForMember(destination => destination.Lotes, options => options.Ignore());
    }
}
