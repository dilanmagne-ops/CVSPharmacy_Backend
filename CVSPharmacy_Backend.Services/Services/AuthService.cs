using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public sealed class AuthService(IUnitOfWork unitOfWork, IVerificadorPassword verificador) : IAuthService
{
    public async Task<Usuario> AutenticarAsync(string email, string password)
    {
        var usuario = await unitOfWork.UsuarioRepository.GetPorEmailConRolAsync(email.Trim());
        if (!verificador.Verificar(password, usuario?.PasswordHash) || usuario is null)
            throw new BusinessException("Credenciales inválidas", HttpStatusCode.Unauthorized);

        VerificarActivo(usuario);
        usuario.UltimoAcceso = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> GetUsuarioActualAsync(int id)
    {
        var usuario = await unitOfWork.UsuarioRepository.GetPorIdConRolAsync(id)
            ?? throw new BusinessException("Usuario no encontrado.", HttpStatusCode.Unauthorized);
        VerificarActivo(usuario);
        return usuario;
    }

    private static void VerificarActivo(Usuario usuario)
    {
        if (usuario.Activo != true)
            throw new BusinessException("Usuario inactivo, contacte al administrador", HttpStatusCode.Forbidden);
    }
}
