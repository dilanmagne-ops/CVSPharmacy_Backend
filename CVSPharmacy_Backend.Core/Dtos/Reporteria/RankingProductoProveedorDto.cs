namespace CVSPharmacy_Backend.Core.Dtos.Reporteria;

public sealed class RankingProductoProveedorDto
{
    public int ProveedorId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public long PosicionEnProveedor { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public long TotalUnidadesRecibidas { get; set; }
}
