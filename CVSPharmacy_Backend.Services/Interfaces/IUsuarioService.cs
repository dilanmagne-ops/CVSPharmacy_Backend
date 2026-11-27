using CVSPharmacy_Backend.Core.Dtos.Usuario;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<Usuario>> ListarAsync(int actorId, string? rol, bool? activo);
    Task<Usuario> ObtenerAsync(int actorId, int id);
    Task<Usuario> CrearAsync(int actorId, CrearUsuarioDto dto);
    Task<Usuario> EditarAsync(int actorId, int id, EditarUsuarioDto dto);
    Task<Usuario> CambiarEstadoAsync(int actorId, int id, bool activo);
}
