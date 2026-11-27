namespace CVSPharmacy_Backend.Core.Dtos.Usuario;

public sealed record CrearUsuarioDto(string NombreCompleto, string Email, string Password, string Rol);
public sealed record EditarUsuarioDto(string NombreCompleto, string Rol);
public sealed record EstadoUsuarioDto(bool? Activo);
public sealed record UsuarioDto(int Id, string NombreCompleto, string Email, string Rol, bool Activo);
