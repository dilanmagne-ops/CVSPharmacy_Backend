namespace CVSPharmacy_Backend.Core.Dtos.Auth;

public sealed record UsuarioAutenticadoDto(int Id, string NombreCompleto, string Email, string Rol);
