namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IVerificadorPassword
{
    string GenerarHash(string password);
    bool Verificar(string password, string? hash);
}
