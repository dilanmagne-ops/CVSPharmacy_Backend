namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;

public class LoteRecibidoDto
{
    public int? ProductoId { get; set; }
    public int? Cantidad { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly? FechaVencimiento { get; set; }
}
