namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IDapperContext
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql);
}
