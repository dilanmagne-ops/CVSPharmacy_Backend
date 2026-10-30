using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Results;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IOrdenCompraService
{
    Task<IEnumerable<OrdenCompraDto>> GetAllOrdenesAsync(string? estado, int? proveedorId, DateTime? desde, DateTime? hasta);
    Task<OrdenCompraDto?> GetOrdenByIdAsync(int id);
    Task<ResultadoServicio<OrdenCompraDto>> CrearOrdenAsync(GuardarOrdenCompraDto ordenDto);
    Task<ResultadoServicio<OrdenCompraDto>> ActualizarOrdenAsync(int id, GuardarOrdenCompraDto ordenDto);
    Task<ResultadoServicio<OrdenCompraDto>> EmitirOrdenAsync(int id);
    Task<ResultadoServicio<OrdenCompraDto>> CancelarOrdenAsync(int id);
}
