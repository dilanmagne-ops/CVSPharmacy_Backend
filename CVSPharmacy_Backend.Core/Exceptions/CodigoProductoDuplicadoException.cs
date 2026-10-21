namespace CVSPharmacy_Backend.Core.Exceptions
{
    public class CodigoProductoDuplicadoException : Exception
    {
        public CodigoProductoDuplicadoException(Exception innerException)
            : base("El codigo del producto ya existe.", innerException)
        {
        }
    }
}
