using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class DetalleOrdenCompra
{
    public int Id { get; set; }

    public int OrdenId { get; set; }

    public int ProductoId { get; set; }

    public int CantidadSolicitada { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public virtual OrdenesCompra Orden { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
