using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces
{
    public interface IProductoRepository
    {
        // Consultar productos
        // Consultar todos los productos
        Task<IEnumerable<Producto>> GetAllProductosAsync();
        // Buscar un producto por id
        Task<Producto?> GetProductoByIdAsync(int id);
        // Buscar un producto por id para actualizarlo
        Task<Producto?> GetProductoParaActualizarAsync(int id);
        // Verificar si existe un producto con un código específico RN-06
        Task<Producto?> GetProductoByCodigoAsync(string codigo);

        // Registrar y actualizar productos
        // Registrar un nuevo producto RN-06
        Task InsertProducto(Producto producto);
        // Actualizar la información o estado de un producto RN-07
        Task UpdateProducto(Producto producto);

        // Consultar productos con stock critico RN-08
        Task<IEnumerable<Producto>> GetProductosStockCriticoAsync();
    }
}
