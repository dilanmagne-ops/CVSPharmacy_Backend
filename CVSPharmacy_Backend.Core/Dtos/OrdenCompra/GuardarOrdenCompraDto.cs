namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra;


// Cuerpo del POST y del PUT de /api/purchase-orders
public class GuardarOrdenCompraDto
{
    public int? ProveedorId { get; set; }

    public string? Observaciones { get; set; }

    public List<ItemOrdenCompraDto> Items { get; set; } = new();
}

public class ItemOrdenCompraDto
{
    public int? ProductoId { get; set; }

    public int? CantidadSolicitada { get; set; }

    public decimal? PrecioUnitario { get; set; }
}

