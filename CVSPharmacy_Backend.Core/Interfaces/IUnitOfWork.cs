namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUnitOfWork
{
    IProductoRepository ProductoRepository { get; }
    IProveedorRepository ProveedorRepository { get; }
    Task SaveChangesAsync();
}
