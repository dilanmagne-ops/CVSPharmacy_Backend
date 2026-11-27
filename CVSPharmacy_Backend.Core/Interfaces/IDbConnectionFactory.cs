using System.Data.Common;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
