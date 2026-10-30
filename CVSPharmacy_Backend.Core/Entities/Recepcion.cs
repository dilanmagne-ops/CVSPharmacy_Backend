using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class Recepcion : BaseEntity
{
    public int OrdenId { get; set; }

    public int RecibidoPor { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public string? NroRemision { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public virtual OrdenesCompra Orden { get; set; } = null!;

    public virtual Usuario RecibidoPorNavigation { get; set; } = null!;
}
