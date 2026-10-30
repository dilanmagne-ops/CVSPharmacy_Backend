namespace CVSPharmacy_Backend.Services.Interfaces;
public record UsuarioOperacion(int Id, string Rol);
public interface IUsuarioActual
{
    Task<UsuarioOperacion> ObtenerAsync(bool soloAdministrador = false);
}
