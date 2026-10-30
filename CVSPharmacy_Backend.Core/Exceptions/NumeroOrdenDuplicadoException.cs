namespace CVSPharmacy_Backend.Core.Exceptions;

public class NumeroOrdenDuplicadoException : Exception
{
    public NumeroOrdenDuplicadoException(Exception inner)
        : base("El número de orden ya existe.", inner)
    {
    }
}
