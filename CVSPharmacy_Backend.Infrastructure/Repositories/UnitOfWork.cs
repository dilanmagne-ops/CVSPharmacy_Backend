using System.Data;
using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public class UnitOfWork(FarmaciaDbContext context, IProductoRepository productoRepository,
    IProveedorRepository proveedorRepository, IOrdenCompraRepository ordenCompraRepository,
    IBaseRepository<Usuario> usuarioRepository, IRecepcionRepository recepcionRepository,
    IReportesRepository reportesRepository) : IUnitOfWork
{
    public IReportesRepository ReportesRepository { get; } = reportesRepository;

    public IProductoRepository ProductoRepository { get; } = productoRepository;

    public IProveedorRepository ProveedorRepository { get; } = proveedorRepository;

    public IOrdenCompraRepository OrdenCompraRepository { get; } = ordenCompraRepository;
    public IBaseRepository<Usuario> UsuarioRepository { get; } = usuarioRepository;

    public IRecepcionRepository RecepcionRepository { get; } = recepcionRepository;

    public async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion)
    {
        await using var transaccion = await context.Database
            .BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var resultado = await operacion();
            await transaccion.CommitAsync();
            return resultado;
        }
        catch (MySqlException ex) when (ex.Number is 1205 or 1213)
        {
            await transaccion.RollbackAsync();
            context.ChangeTracker.Clear();
            throw new BusinessException(
                "La orden esta siendo modificada. Intente nuevamente.",
                HttpStatusCode.Conflict);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is MySqlException { Number: 1205 or 1213 })
        {
            await transaccion.RollbackAsync();
            context.ChangeTracker.Clear();
            throw new BusinessException(
                "La orden esta siendo modificada. Intente nuevamente.",
                HttpStatusCode.Conflict);
        }
        catch
        {
            await transaccion.RollbackAsync();
            context.ChangeTracker.Clear();
            throw;
        }
    }

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
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 } mysql
            && mysql.Message.Contains("uq_lotes_producto_numero", StringComparison.OrdinalIgnoreCase))
        {
            throw new NumeroLoteDuplicadoException(ex);
        }
    }
}
