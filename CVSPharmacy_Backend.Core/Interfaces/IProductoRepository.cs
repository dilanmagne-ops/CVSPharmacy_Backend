using CVSPharmacy_Backend.Core.Dtos.Producto;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IProductoRepository : IBaseRepository<Producto>
{
    Task<List<Producto>> GetProductosAsync(IEnumerable<int> ids);
    Task<Producto?> GetProductoParaActualizarAsync(int id);

    Task<Producto?> GetProductoByCodigoAsync(string codigo);

    Task<IEnumerable<Producto>> GetProductosStockCriticoAsync();
}
