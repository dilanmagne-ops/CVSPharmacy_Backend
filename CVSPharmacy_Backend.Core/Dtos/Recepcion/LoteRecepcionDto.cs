namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;

public class LoteRecepcionDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int DetalleOrdenId { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public DateOnly FechaVencimiento { get; set; }
    public int CantidadRecibida { get; set; }
    public int CantidadDisponible { get; set; }
}
