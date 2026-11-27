using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IRecepcionService
{
    Task<IEnumerable<Recepcion>> GetRecepcionesAsync(
        int ordenId, RecepcionQueryFilters filters);

    Task<Recepcion> RegistrarRecepcionAsync(
        int ordenId, Recepcion recepcion, int usuarioId);
}
