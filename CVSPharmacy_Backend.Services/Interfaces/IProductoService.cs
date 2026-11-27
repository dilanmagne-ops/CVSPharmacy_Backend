using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<Producto>> GetAllProductosAsync(ProductoQueryFilters filters);
    Task<Producto> GetProductoByIdAsync(int id);
    Task<IEnumerable<Producto>> GetProductosStockCriticoAsync();
    Task<Producto> CrearProductoAsync(Producto producto);
    Task<Producto> ActualizarProductoAsync(int id, Producto cambios);
    Task<bool> CambiarEstadoProductoAsync(int id, bool activo);
}
