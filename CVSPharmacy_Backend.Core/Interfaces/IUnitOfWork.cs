using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository ProductoRepository { get; }
    IProveedorRepository ProveedorRepository { get; }
    IOrdenCompraRepository OrdenCompraRepository { get; }
    IUsuarioRepository UsuarioRepository { get; }
    IRecepcionRepository RecepcionRepository { get; }
    Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion);
    Task SaveChangesAsync();
}
