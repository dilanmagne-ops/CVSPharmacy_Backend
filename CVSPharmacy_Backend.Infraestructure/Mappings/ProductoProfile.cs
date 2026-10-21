using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Producto;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Infraestructure.Mappings
{
    public class ProductoProfile : Profile
    {
        public ProductoProfile()
        {
            // Entidad -> DTO de lectura
            //Utilizacion Consultas GET
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.StockDisponible,
                    opt => opt.Ignore())
                .ForMember(dest => dest.EsStockCritico,
                    opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

                    int stock = src.Lotes
                        .Where(l => l.FechaVencimiento >= hoy
                                 && l.CantidadDisponible > 0)
                        .Sum(l => l.CantidadDisponible);

                    dest.StockDisponible = stock;
                    dest.EsStockCritico = stock <= src.StockMinimo;
                });

            // DTO de escritura -> Entidad
            // Utilizacion Consultas POST y PUT
            CreateMap<GuardarProductoDto, Producto>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Activo,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreadoEn,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Lotes,
                    opt => opt.Ignore())
                .ForMember(dest => dest.DetalleOrdenCompras,
                    opt => opt.Ignore());
        }
    }
}
