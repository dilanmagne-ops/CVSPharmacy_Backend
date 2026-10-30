using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IOrdenCompraRepository
{
    // Consultas (sin tracking, con proveedor, ítems y productos)
    Task<IEnumerable<OrdenesCompra>> GetAllOrdenesAsync(string? estado, int? proveedorId, DateTime? desde, DateTime? hasta);
    Task<OrdenesCompra?> GetOrdenByIdAsync(int id);

    // Con tracking y con ítems, para modificar
    Task<OrdenesCompra?> GetOrdenParaActualizarAsync(int id);

    // Datos de apoyo para validar RN-10
    Task<Proveedor?> GetProveedorAsync(int id);
    Task<List<Producto>> GetProductosAsync(IEnumerable<int> ids);

    Task<string> GenerarNumeroAsync();
    Task<bool> TieneRecepcionesAsync(int ordenId);

    // Escritura
    Task InsertOrdenAsync(OrdenesCompra orden);
    void ReemplazarDetalles(OrdenesCompra orden, IEnumerable<DetalleOrdenCompra> nuevos);
    Task SaveAsync();
}
