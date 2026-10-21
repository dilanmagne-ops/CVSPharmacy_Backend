namespace CVSPharmacy_Backend.Core.Dtos;

public sealed class ProveedorDto
{
    public int Id { get; init; }
    public string Nit { get; init; } = string.Empty;
    public string RazonSocial { get; init; } = string.Empty;
    public string? Contacto { get; init; }
    public string? Telefono { get; init; }
    public string? Email { get; init; }
    public string? Direccion { get; init; }
    public bool Activo { get; init; }
    public DateTime CreadoEn { get; init; }
}
