using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces
{
    public interface IOrdenCompraRepository : IBaseRepository<OrdenesCompra>
    {
        Task<OrdenesCompra?> GetOrdenParaActualizarAsync(int id);

        Task<Proveedor?> GetProveedorAsync(int id);

        Task<List<Producto>> GetProductosAsync(IEnumerable<int> ids);

        Task<bool> TieneRecepcionesAsync(int ordenId);

        void ReemplazarDetalles(
            OrdenesCompra orden,
            IEnumerable<DetalleOrdenCompra> nuevosDetalles);
        Task<Usuario?> GetUsuarioAsync(int id);

        Task<List<string>> GetNumerosOrdenAsync(string prefijo);
        Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion);
    }
}
