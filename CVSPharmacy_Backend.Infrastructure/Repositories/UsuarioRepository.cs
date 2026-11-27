using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public sealed class UsuarioRepository(FarmaciaDbContext context) : BaseRepository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> GetPorEmailConRolAsync(string email) =>
        _entities.Include(u => u.Rol).SingleOrDefaultAsync(u => u.Email == email);

    public Task<Usuario?> GetPorIdConRolAsync(int id) =>
        _entities.AsNoTracking().Include(u => u.Rol).SingleOrDefaultAsync(u => u.Id == id);
}
