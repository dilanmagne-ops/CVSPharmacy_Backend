using CVSPharmacy_Backend.Core.Dtos.Producto;

namespace CVSPharmacy_Backend.Services.Interfaces
{
    public interface IProductoService
    {
        // Consultas
        Task<IEnumerable<ProductoDto>> GetAllProductosAsync(string? q, bool? active);
        Task<ProductoDto> GetProductoByIdAsync(int id);
        Task<IEnumerable<ProductoDto>> GetProductosStockCriticoAsync();

        // Registro
        Task<ProductoDto> CrearProductoAsync(GuardarProductoDto productoDto);

        // Actualizacion
        Task<ProductoDto> ActualizarProductoAsync(int id, GuardarProductoDto productoDto);

        // Cambio de estado
        Task<bool> CambiarEstadoProductoAsync(int id, CambiarEstadoProductoDto estadoDto);
    }
}
