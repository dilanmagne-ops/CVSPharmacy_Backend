using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository ProductoRepository { get; }
    IProveedorRepository ProveedorRepository { get; }
    IOrdenCompraRepository OrdenCompraRepository { get; }
    IBaseRepository<Usuario> UsuarioRepository { get; }
    Task SaveChangesAsync();
}
