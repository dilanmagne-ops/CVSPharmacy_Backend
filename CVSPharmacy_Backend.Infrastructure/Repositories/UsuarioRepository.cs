using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infrastructure.Repositories;

public sealed class UsuarioRepository(FarmaciaDbContext context) : BaseRepository<Usuario>(context), IUsuarioRepository
{
    public async Task<IReadOnlyList<Usuario>> ListarAsync(string? rol, bool? activo)
    {
        var consulta = _entities.AsNoTracking().Include(u => u.Rol).AsQueryable();
        if (rol != null) consulta = consulta.Where(u => u.Rol.Nombre == rol);
        if (activo.HasValue) consulta = consulta.Where(u => u.Activo == activo);
        return await consulta.OrderBy(u => u.Id).ToListAsync();
    }

    public Task<Rol?> BuscarRolAsync(string nombre) =>
        _context.Roles.SingleOrDefaultAsync(r => r.Nombre == nombre);

    // Todas las mutaciones del CU-02 toman la misma fila antes de leer usuarios.
    public async Task BloquearAdministracionAsync() =>
        await _context.Roles.FromSqlRaw("SELECT * FROM roles WHERE nombre = 'Administrador' FOR UPDATE")
            .AsNoTracking().ToListAsync();

    public Task<bool> ExisteOtroAdministradorActivoAsync(int id) =>
        _entities.AnyAsync(u => u.Id != id && u.Activo == true && u.Rol.Nombre == "Administrador");

    public Task<Usuario?> GetPorEmailConRolAsync(string email) =>
        _entities.Include(u => u.Rol).SingleOrDefaultAsync(u => u.Email == email);

    public Task<Usuario?> GetPorIdConRolAsync(int id) =>
        _entities.AsNoTracking().Include(u => u.Rol).SingleOrDefaultAsync(u => u.Id == id);
}
