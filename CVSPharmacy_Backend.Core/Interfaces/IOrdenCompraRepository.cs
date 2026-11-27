using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces
{
    public interface IOrdenCompraRepository : IBaseRepository<OrdenesCompra>
    {
        Task<OrdenesCompra?> GetOrdenParaActualizarAsync(int id);

        Task<bool> TieneRecepcionesAsync(int ordenId);

        void ReemplazarDetalles(
            OrdenesCompra orden,
            IEnumerable<DetalleOrdenCompra> nuevosDetalles);

        Task<List<string>> GetNumerosOrdenAsync(string prefijo);
    }
}
