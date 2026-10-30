namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra;

// Lo que devuelve la API
public class OrdenCompraDto
{
    public int Id { get; set; }

    public string Numero { get; set; } = null!;

    public int ProveedorId { get; set; }

    public string ProveedorRazonSocial { get; set; } = null!;

    public int CreadoPor { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEmision { get; set; }

    public string? Observaciones { get; set; }

    public decimal Total { get; set; }

    public List<DetalleOrdenCompraDto> Items { get; set; } = new();
}

public class DetalleOrdenCompraDto
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public string ProductoCodigo { get; set; } = null!;

    public string ProductoNombre { get; set; } = null!;

    public int CantidadSolicitada { get; set; }

    public decimal PrecioUnitario { get; set; }
}
