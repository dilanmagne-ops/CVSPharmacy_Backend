using System.Data;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
namespace CVSPharmacy_Backend.Infraestructure.Data;

public sealed class UnidadTrabajo(FarmaciaDbContext context) : IUnidadTrabajo
{
    public async Task<T> EjecutarAsync<T>(Func<Task<T>> operacion)
    {
        // Todas las mutaciones de la misma orden utilizan este límite transaccional.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var result = await operacion();
            await transaction.CommitAsync();
            return result;
        }
        catch (MySqlException ex) when (ex.Number is 1205 or 1213)
        {
            await transaction.RollbackAsync(); context.ChangeTracker.Clear();
            throw new BusinessException("La orden está siendo modificada. Reintente la operación.", System.Net.HttpStatusCode.Conflict);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1205 or 1213 })
        {
            await transaction.RollbackAsync(); context.ChangeTracker.Clear();
            throw new BusinessException("La orden está siendo modificada. Reintente la operación.", System.Net.HttpStatusCode.Conflict);
        }
        catch
        {
            await transaction.RollbackAsync(); context.ChangeTracker.Clear(); throw;
        }
    }

    public async Task<OrdenesCompra?> BloquearOrdenAsync(int id)
    {
        // Parámetro interpolado por EF: no se concatena SQL recibido del cliente.
        // ToListAsync evita componer otra consulta alrededor del FOR UPDATE.
        var rows = await context.OrdenesCompras.FromSqlInterpolated(
            $"SELECT * FROM ordenes_compra WHERE id = {id} FOR UPDATE").ToListAsync();
        return rows.SingleOrDefault();
    }
}
