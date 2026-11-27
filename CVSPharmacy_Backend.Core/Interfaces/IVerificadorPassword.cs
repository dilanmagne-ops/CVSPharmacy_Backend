namespace CVSPharmacy_Backend.Core.Interfaces;

public interface IVerificadorPassword
{
    bool Verificar(string password, string? hash);
}
