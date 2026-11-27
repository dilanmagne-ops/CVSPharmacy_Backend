using CVSPharmacy_Backend.Core.Dtos.Recepcion;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IRecepcionService
{
    Task<IEnumerable<RecepcionDto>> GetRecepcionesAsync(
        int ordenId, DateTime? from = null, DateTime? to = null);

    Task<RecepcionDto> RegistrarRecepcionAsync(
        int ordenId, GuardarRecepcionDto recepcionDto, int usuarioId);
}
