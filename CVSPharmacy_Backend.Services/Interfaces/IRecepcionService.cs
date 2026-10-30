using CVSPharmacy_Backend.Core.Dtos.Recepcion;
namespace CVSPharmacy_Backend.Services.Interfaces;
public interface IRecepcionService
{
    Task<RecepcionDto> RegistrarAsync(int ordenId, GuardarRecepcionDto dto);
    Task<IReadOnlyList<RecepcionDto>> ConsultarAsync(int ordenId);
}
