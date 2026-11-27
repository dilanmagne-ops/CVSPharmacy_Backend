namespace CVSPharmacy_Backend.Core.CustomEntities.Reporteria;

public sealed class RankingProductoProveedor
{
    public int ProveedorId { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public long PosicionEnProveedor { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public long TotalUnidadesRecibidas { get; set; }
}
