using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IUsuarioRepository : IBaseRepository<Usuario>
{
    Task<IReadOnlyList<Usuario>> ListarAsync(string? rol, bool? activo);
    Task<Rol?> BuscarRolAsync(string nombre);
    Task BloquearAdministracionAsync();
    Task<bool> ExisteOtroAdministradorActivoAsync(int id);
    Task<Usuario?> GetPorEmailConRolAsync(string email);
    Task<Usuario?> GetPorIdConRolAsync(int id);
}
