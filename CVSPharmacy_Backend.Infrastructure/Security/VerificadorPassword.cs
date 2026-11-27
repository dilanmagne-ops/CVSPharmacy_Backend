using System.Text;
using CVSPharmacy_Backend.Core.Interfaces;

namespace CVSPharmacy_Backend.Infrastructure.Security;

public sealed class VerificadorPassword : IVerificadorPassword
{
    // Ejecutar bcrypt también cuando el correo no tiene una cuenta asociada.
    private static readonly string HashDePrueba = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));

    public string GenerarHash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verificar(string password, string? hash)
    {
        if (Encoding.UTF8.GetByteCount(password) > 72) return false;
        try
        {
            var valido = BCrypt.Net.BCrypt.Verify(password, hash is { Length: 60 } ? hash : HashDePrueba);
            return hash is { Length: 60 } && valido;
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or FormatException or ArgumentException)
        {
            return false;
        }
    }
}
