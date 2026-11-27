using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUnitOfWork
{
    IReportesRepository ReportesRepository { get; }
    IProductoRepository ProductoRepository { get; }
    IProveedorRepository ProveedorRepository { get; }
    IOrdenCompraRepository OrdenCompraRepository { get; }
    IBaseRepository<Usuario> UsuarioRepository { get; }
    IRecepcionRepository RecepcionRepository { get; }
    Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion);
    Task SaveChangesAsync();
}
