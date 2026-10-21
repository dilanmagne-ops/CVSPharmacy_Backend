namespace CVSPharmacy_Backend.Core.Dtos.Proveedor;

public sealed class GuardarProveedorDto
{
    public string Nit { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
}
