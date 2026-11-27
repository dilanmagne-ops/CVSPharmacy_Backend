using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IOrdenCompraService
{
    Task<IEnumerable<OrdenesCompra>> GetAllOrdenesAsync(OrdenCompraQueryFilters filters);
    Task<OrdenesCompra> GetOrdenByIdAsync(int id);
    Task<OrdenesCompra> CrearOrdenAsync(OrdenesCompra solicitud, int usuarioId);
    Task<OrdenesCompra> ActualizarOrdenAsync(int id, OrdenesCompra solicitud);
    Task<OrdenesCompra> EmitirOrdenAsync(int id);
    Task<OrdenesCompra> CancelarOrdenAsync(int id);
}
