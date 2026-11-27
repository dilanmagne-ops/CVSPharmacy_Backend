using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CVSPharmacy_Backend.Api.Security;

public static class PermisosFarmacia
{
    public const string PersonalFarmacia = nameof(PersonalFarmacia);

    public static void Registrar(IServiceCollection servicios)
    {
        servicios.AddAuthorization(options => options.AddPolicy(PersonalFarmacia, policy =>
            policy.RequireAuthenticatedUser()
                .RequireRole("Administrador", "Operador")
                .AddRequirements(new CuentaVigente())));
        servicios.AddScoped<IAuthorizationHandler, VerificarCuentaVigente>();
    }

    public static int ObtenerIdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) && id > 0
            ? id : throw new BusinessException("Identidad inválida.", HttpStatusCode.Unauthorized);
}

internal sealed class CuentaVigente : IAuthorizationRequirement { }

internal sealed class VerificarCuentaVigente(IAuthService autenticacion) : AuthorizationHandler<CuentaVigente>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CuentaVigente requisito)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;
        var usuario = await autenticacion.GetUsuarioActualAsync(context.User.ObtenerIdUsuario());
        if (context.User.IsInRole(usuario.Rol.Nombre))
            context.Succeed(requisito);
    }
}
