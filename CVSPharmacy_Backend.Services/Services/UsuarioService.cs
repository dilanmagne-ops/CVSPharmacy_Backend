using System.Net;
using CVSPharmacy_Backend.Core.Dtos.Usuario;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public sealed class UsuarioService(IUnitOfWork unidad, IVerificadorPassword passwords) : IUsuarioService
{
    private IUsuarioRepository Usuarios => unidad.UsuarioRepository;

    public async Task<IReadOnlyList<Usuario>> ListarAsync(int actorId, string? rol, bool? activo)
    {
        await ExigirAdministradorAsync(actorId);
        return await Usuarios.ListarAsync(rol?.Trim(), activo);
    }

    public async Task<Usuario> ObtenerAsync(int actorId, int id)
    {
        await ExigirAdministradorAsync(actorId);
        return await LeerConRolAsync(id);
    }

    public Task<Usuario> CrearAsync(int actorId, CrearUsuarioDto dto) =>
        ModificarAsync(actorId, async () =>
        {
            var email = dto.Email.Trim();
            if (await Usuarios.GetPorEmailConRolAsync(email) != null)
                throw new BusinessException("Correo ya registrado", HttpStatusCode.Conflict);
            var rol = await ResolverRolAsync(dto.Rol);
            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto.Trim(),
                Email = email,
                Rol = rol,
                Activo = true,
                PasswordHash = passwords.GenerarHash(dto.Password)
            };
            await Usuarios.Add(usuario);
            return usuario;
        });

    public Task<Usuario> EditarAsync(int actorId, int id, EditarUsuarioDto dto) =>
        ModificarAsync(actorId, async () =>
        {
            var usuario = await Usuarios.GetById(id) ?? throw NoEncontrado();
            var rol = await ResolverRolAsync(dto.Rol);
            if (rol.Id != usuario.RolId && rol.Nombre != "Administrador")
                await ProtegerUltimoAdministradorAsync(usuario);
            usuario.NombreCompleto = dto.NombreCompleto.Trim();
            usuario.Rol = rol;
            return usuario;
        });

    public Task<Usuario> CambiarEstadoAsync(int actorId, int id, bool activo) =>
        ModificarAsync(actorId, async () =>
        {
            var usuario = await Usuarios.GetById(id) ?? throw NoEncontrado();
            if (!activo)
            {
                if (actorId == id)
                    throw new BusinessException("No puede desactivar su propia cuenta.", HttpStatusCode.Conflict);
                await ProtegerUltimoAdministradorAsync(usuario);
            }
            usuario.Activo = activo;
            return usuario;
        });

    private Task<Usuario> ModificarAsync(int actorId, Func<Task<Usuario>> modificar) =>
        unidad.EjecutarEnTransaccionAsync(async () =>
        {
            await Usuarios.BloquearAdministracionAsync();
            await ExigirAdministradorAsync(actorId);
            var usuario = await modificar();
            await unidad.SaveChangesAsync();
            return await LeerConRolAsync(usuario.Id);
        });

    private async Task ExigirAdministradorAsync(int actorId)
    {
        var actor = await Usuarios.GetPorIdConRolAsync(actorId);
        if (actor?.Activo != true || actor.Rol.Nombre != "Administrador")
            throw new BusinessException("Se requiere un administrador activo.", HttpStatusCode.Forbidden);
    }

    private async Task ProtegerUltimoAdministradorAsync(Usuario usuario)
    {
        if (usuario.Activo == true && (await LeerConRolAsync(usuario.Id)).Rol.Nombre == "Administrador"
            && !await Usuarios.ExisteOtroAdministradorActivoAsync(usuario.Id))
            throw new BusinessException("Debe quedar al menos un administrador activo.", HttpStatusCode.Conflict);
    }

    private async Task<Rol> ResolverRolAsync(string nombre) =>
        await Usuarios.BuscarRolAsync(nombre.Trim()) ?? throw new BusinessException("El rol no existe.");

    private async Task<Usuario> LeerConRolAsync(int id) =>
        await Usuarios.GetPorIdConRolAsync(id) ?? throw NoEncontrado();

    private static BusinessException NoEncontrado() => new("Usuario no encontrado.", HttpStatusCode.NotFound);
}
