using CVSPharmacy_Backend.Core.Entities;
namespace CVSPharmacy_Backend.Core.Interfaces;
public interface IUnidadTrabajo
{
    Task<T> EjecutarAsync<T>(Func<Task<T>> operacion);
    Task<OrdenesCompra?> BloquearOrdenAsync(int id);
}
