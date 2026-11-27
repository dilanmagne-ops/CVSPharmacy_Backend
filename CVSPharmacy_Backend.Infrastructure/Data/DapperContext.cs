using CVSPharmacy_Backend.Core.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CVSPharmacy_Backend.Infrastructure.Data;

public sealed class DapperContext(IDbConnectionFactory factory, FarmaciaDbContext context) : IDapperContext
{
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
    {
        if (context.Database.CurrentTransaction is { } transaccion)
            return await context.Database.GetDbConnection().QueryAsync<T>(sql, transaction: transaccion.GetDbTransaction());

        await using var conexion = factory.CreateConnection();
        return await conexion.QueryAsync<T>(sql);
    }
}
