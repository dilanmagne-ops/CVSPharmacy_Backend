using System.Linq.Expressions;
using CVSPharmacy_Backend.Core.Exceptions;
using MySqlConnector;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infraestructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly FarmaciaDbContext Context;
    protected readonly DbSet<T> Entities;

    public BaseRepository(FarmaciaDbContext context)
    {
        Context = context;
        Entities = context.Set<T>();
    }

    private IQueryable<T> ReadQuery(Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = Entities.AsNoTracking();
        foreach (var include in includes) query = query.Include(include);
        return query;
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] includes)
    {
        var query = ReadQuery(includes);
        if (predicate is not null) query = query.Where(predicate);
        return await query.ToListAsync();
    }

    public virtual Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes) =>
        ReadQuery(includes).FirstOrDefaultAsync(e => e.Id == id);

    public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        Entities.AsNoTracking().FirstOrDefaultAsync(predicate);

    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => Entities.AnyAsync(predicate);

    public virtual Task<T?> GetForUpdateAsync(int id) => Entities.FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task AddAsync(T entity) => await Entities.AddAsync(entity);

    public virtual Task UpdateAsync(T entity)
    {
        Entities.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetForUpdateAsync(id);
        if (entity is not null) Entities.Remove(entity);
    }

    public virtual async Task SaveAsync()
    {
        try { await Context.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        {
            throw new DuplicateKeyException(ex);
        }
    }
}
