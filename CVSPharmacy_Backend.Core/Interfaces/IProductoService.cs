using CVSPharmacy_Backend.Core.Dtos;
using CVSPharmacy_Backend.Core.Results;

namespace CVSPharmacy_Backend.Core.Interfaces
{
    public interface IProductoService
    {
        // Consultas
        Task<IEnumerable<ProductoDto>> GetAllProductosAsync(string? q, bool? active);
        Task<ProductoDto?> GetProductoByIdAsync(int id);
        Task<IEnumerable<ProductoDto>> GetProductosStockCriticoAsync();

        // Registro
        Task<ResultadoServicio<ProductoDto>> CrearProductoAsync(GuardarProductoDto productoDto);

        // Actualizacion
        Task<ResultadoServicio<ProductoDto>> ActualizarProductoAsync(int id, GuardarProductoDto productoDto);

        // Cambio de estado
        Task<ResultadoServicio<bool>> CambiarEstadoProductoAsync(int id, CambiarEstadoProductoDto estadoDto);
    }
}
