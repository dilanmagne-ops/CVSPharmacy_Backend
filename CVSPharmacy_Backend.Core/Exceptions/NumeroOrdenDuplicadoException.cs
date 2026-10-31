namespace CVSPharmacy_Backend.Core.Exceptions;

public class NumeroOrdenDuplicadoException : Exception
{
    public NumeroOrdenDuplicadoException(Exception innerException)
        : base("El numero de orden ya existe.", innerException)
    {
    }
}
