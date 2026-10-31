namespace CVSPharmacy_Backend.Core.Dtos.Recepcion;

public class GuardarRecepcionDto
{
    public string? NroRemision { get; set; }
    public string? Observaciones { get; set; }
    public List<LoteRecibidoDto> Items { get; set; } = new();
}
