using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class OrdenesCompra : BaseEntity
{
    public string Numero { get; set; } = null!;

    public int ProveedorId { get; set; }

    public int CreadoPor { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEmision { get; set; }

    public string? Observaciones { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual ICollection<DetalleOrdenCompra> DetalleOrdenCompras { get; set; } = new List<DetalleOrdenCompra>();

    public virtual Proveedor Proveedor { get; set; } = null!;

    public virtual ICollection<Recepcion> Recepciones { get; set; } = new List<Recepcion>();
}
