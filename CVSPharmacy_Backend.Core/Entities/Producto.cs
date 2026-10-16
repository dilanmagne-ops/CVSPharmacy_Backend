using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string PrincipioActivo { get; set; } = null!;

    public string? Presentacion { get; set; }

    public string UnidadMedida { get; set; } = null!;

    public decimal PrecioVenta { get; set; }

    public sbyte StockMinimo { get; set; }

    public sbyte Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Detalleordencompra> Detalleordencompras { get; set; } = new List<Detalleordencompra>();

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public virtual ICollection<Ordencompra> Ordencompras { get; set; } = new List<Ordencompra>();
}
