namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;
public sealed class GuardarRecepcionDto
{
    public string? NroRemision { get; set; }
    public string? Observaciones { get; set; }
    public List<LoteRecibidoDto> Items { get; set; } = new();
}
public sealed class LoteRecibidoDto
{
    public int? ProductoId { get; set; }
    public int? Cantidad { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly? FechaVencimiento { get; set; }
}
