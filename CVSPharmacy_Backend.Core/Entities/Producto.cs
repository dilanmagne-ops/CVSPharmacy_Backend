using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class Producto : BaseEntity
{

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? PrincipioActivo { get; set; }

    public string Presentacion { get; set; } = null!;

    public string UnidadMedida { get; set; } = null!;

    public decimal PrecioVenta { get; set; }

    public int StockMinimo { get; set; }

    public bool? Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<DetalleOrdenCompra> DetalleOrdenCompras { get; set; } = new List<DetalleOrdenCompra>();

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}
