namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;
public sealed class RecepcionDto
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int RecibidoPor { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public string? NroRemision { get; set; }
    public string? Observaciones { get; set; }
    public List<LoteRecepcionDto> Lotes { get; set; } = new();
}
public sealed class LoteRecepcionDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int DetalleOrdenId { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public int CantidadRecibida { get; set; }
    public int CantidadDisponible { get; set; }
}
