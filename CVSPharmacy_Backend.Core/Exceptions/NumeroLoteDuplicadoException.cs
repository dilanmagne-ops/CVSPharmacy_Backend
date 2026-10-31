namespace CVSPharmacy_Backend.Core.Exceptions;

public class NumeroLoteDuplicadoException : Exception
{
    public NumeroLoteDuplicadoException(Exception innerException)
        : base("El numero de lote ya existe para el producto.", innerException)
    {
    }
}
