namespace CVSPharmacy_Backend.Core.Exceptions;

public class NitProveedorDuplicadoException : Exception
{
    public NitProveedorDuplicadoException(Exception innerException)
        : base("El NIT del proveedor ya existe.", innerException)
    {
    }
}
