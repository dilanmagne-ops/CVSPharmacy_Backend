namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;

public class RecepcionDto
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int RecibidoPor { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public string? NroRemision { get; set; }
    public string? Observaciones { get; set; }
    public List<LoteRecepcionDto> Lotes { get; set; } = new();
}
