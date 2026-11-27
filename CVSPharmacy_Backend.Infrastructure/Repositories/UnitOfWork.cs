using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class UnitOfWork(FarmaciaDbContext context, IProductoRepository productoRepository,
    IProveedorRepository proveedorRepository, IOrdenCompraRepository ordenCompraRepository,
    IBaseRepository<Usuario> usuarioRepository) : IUnitOfWork
{
    public IProductoRepository ProductoRepository { get; } = productoRepository;

    public IProveedorRepository ProveedorRepository { get; } = proveedorRepository;

    public IOrdenCompraRepository OrdenCompraRepository { get; } = ordenCompraRepository;
    public IBaseRepository<Usuario> UsuarioRepository { get; } = usuarioRepository;

    public async Task SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 }
            && ex.Entries.Any(entry => entry.Entity is Producto))
        {
            throw new CodigoProductoDuplicadoException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 }
            && ex.Entries.Any(entry => entry.Entity is Proveedor))
        {
            throw new NitProveedorDuplicadoException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 } mysql
            && mysql.Message.Contains("uq_ordenes_compra_numero", StringComparison.OrdinalIgnoreCase))
        {
            context.ChangeTracker.Clear();
            throw new NumeroOrdenDuplicadoException(ex);
        }
    }
}
