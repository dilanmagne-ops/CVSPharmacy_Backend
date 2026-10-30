using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
namespace CVSPharmacy_Backend.Services.Interfaces;
public interface IOrdenCompraService
{
    Task<IEnumerable<OrdenCompraDto>> GetAllOrdenesAsync(string? estado, int? proveedorId, DateTime? desde, DateTime? hasta);
    Task<OrdenCompraDto> GetOrdenByIdAsync(int id);
    Task<OrdenCompraDto> CrearOrdenAsync(GuardarOrdenCompraDto dto);
    Task<OrdenCompraDto> ActualizarOrdenAsync(int id, GuardarOrdenCompraDto dto);
    Task<OrdenCompraDto> EmitirOrdenAsync(int id);
    Task<OrdenCompraDto> CancelarOrdenAsync(int id);
}
