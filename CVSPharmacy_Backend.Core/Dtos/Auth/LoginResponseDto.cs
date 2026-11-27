namespace CVSPharmacy_Backend.Core.Dtos.Auth;

public sealed record LoginResponseDto(string Token, DateTime ExpiraEn, UsuarioAutenticadoDto Usuario);
