using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Services.Interfaces;

public interface IAuthService
{
    Task<Usuario> AutenticarAsync(string email, string password);
    Task<Usuario> GetUsuarioActualAsync(int id);
}
