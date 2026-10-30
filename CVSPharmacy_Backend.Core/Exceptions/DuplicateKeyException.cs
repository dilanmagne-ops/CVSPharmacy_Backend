namespace CVSPharmacy_Backend.Core.Exceptions;

public sealed class DuplicateKeyException : Exception
{
    public DuplicateKeyException(Exception innerException)
        : base("Ya existe un registro con esa clave única.", innerException) { }
}
