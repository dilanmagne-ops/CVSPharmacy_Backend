using System.Linq.Expressions;

using CVSPharmacy_Backend.Core.Entities;



namespace CVSPharmacy_Backend.Core.Interfaces;



public interface IBaseRepository<T> where T : BaseEntity

{

    Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    Task<T?> GetForUpdateAsync(int id);

    // Las escrituras preparan cambios; SaveAsync los confirma.

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(int id);

    void RemoveRange(IEnumerable<T> entities);
    Task SaveAsync();

}
