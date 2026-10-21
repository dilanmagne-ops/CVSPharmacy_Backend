namespace CVSPharmacy_Backend.Core.Results
{
    public enum EstadoResultado
    {
        Exito,
        Validacion,
        NoEncontrado,
        Conflicto
    }

    public class ErrorCampo
    {
        public string Campo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
    }

    public class ResultadoServicio<T>
    {
        public EstadoResultado Estado { get; set; }
            = EstadoResultado.Exito;

        public T? Data { get; set; }

        public string? Mensaje { get; set; }

        public List<ErrorCampo> Errores { get; set; } = new();
    }
}
