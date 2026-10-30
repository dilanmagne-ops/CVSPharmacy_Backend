using System.Net;
using System.Security.Claims;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
namespace CVSPharmacy_Backend.Api.Identity;

public sealed class UsuarioActual(IHttpContextAccessor http, IConfiguration configuration,
    IHostEnvironment environment, IBaseRepository<Usuario> usuarios) : IUsuarioActual
{
    public async Task<UsuarioOperacion> ObtenerAsync(bool soloAdministrador = false)
    {
        var principal = http.HttpContext?.User;
        int id;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out id))
                throw new BusinessException("La sesión no identifica un usuario válido.", HttpStatusCode.Unauthorized);
        }
        else if (environment.IsDevelopment() && configuration.GetValue<int?>("Desarrollo:UsuarioId") is int desarrolloId)
            id = desarrolloId;
        else throw new BusinessException("Debe iniciar sesión para realizar esta operación.", HttpStatusCode.Unauthorized);

        var usuario = await usuarios.GetByIdAsync(id, u => u.Rol);
        if (usuario is null || usuario.Activo != true)
            throw new BusinessException("El usuario no existe o está inactivo.", HttpStatusCode.Forbidden);
        var rol = usuario.Rol.Nombre;
        if (rol != "Administrador" && rol != "Operador")
            throw new BusinessException("Rol no autorizado.", HttpStatusCode.Forbidden);
        if (soloAdministrador && rol != "Administrador")
            throw new BusinessException("Esta operación requiere el rol Administrador.", HttpStatusCode.Forbidden);
        return new UsuarioOperacion(usuario.Id, rol);
    }
}
