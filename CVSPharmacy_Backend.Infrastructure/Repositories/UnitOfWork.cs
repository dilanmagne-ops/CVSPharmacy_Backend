using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class UnitOfWork(FarmaciaDbContext context, IProductoRepository productoRepository,
    IProveedorRepository proveedorRepository) : IUnitOfWork
{
    public IProductoRepository ProductoRepository { get; } = productoRepository;

    public IProveedorRepository ProveedorRepository { get; } = proveedorRepository;

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
    }
}
