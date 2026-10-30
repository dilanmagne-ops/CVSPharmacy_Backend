using System;
using System.Collections.Generic;
using System.Text;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infraestructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly FarmaciaDbContext _context;
    protected readonly DbSet<T> _entities;
    public BaseRepository(FarmaciaDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }
    public virtual async Task<IEnumerable<T>> GetAll()
    {
        return await _entities.ToListAsync();
    }
    public virtual async Task<T?> GetById(int id)
    {
        return await _entities.FindAsync(id);
    }
    public virtual async Task Add(T entity)
    {
        _entities.Add(entity);
        await _context.SaveChangesAsync();
    }
    public virtual async Task Update(T entity)
    {
        _entities.Update(entity);
        await _context.SaveChangesAsync();
    }
    public virtual async Task Delete(int id)
    {
        var entity = await GetById(id);
        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el registro con ID {id}.");
        }
        _entities.Remove(entity);
        await _context.SaveChangesAsync();
    }
}

