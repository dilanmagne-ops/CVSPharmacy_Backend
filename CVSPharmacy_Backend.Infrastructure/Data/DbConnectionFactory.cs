using System.Data.Common;
using CVSPharmacy_Backend.Core.Interfaces;
using MySqlConnector;

namespace CVSPharmacy_Backend.Infrastructure.Data;

public sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public DbConnection CreateConnection() => new MySqlConnection(connectionString);
}
