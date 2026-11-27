namespace CVSPharmacy_Backend.Core.QueryFilters;

public sealed class OrdenCompraQueryFilters
{
    public string? Status { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
