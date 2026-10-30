using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class Lote : BaseEntity
{
    public int RecepcionId { get; set; }

    public int DetalleOrdenId { get; set; }

    public int ProductoId { get; set; }

    public string NumeroLote { get; set; } = null!;

    public DateOnly FechaVencimiento { get; set; }

    public int CantidadRecibida { get; set; }

    public int CantidadDisponible { get; set; }

    public virtual DetalleOrdenCompra DetalleOrden { get; set; } = null!;

    public virtual ICollection<DetalleSalida> DetalleSalida { get; set; } = new List<DetalleSalida>();

    public virtual Producto Producto { get; set; } = null!;

    public virtual Recepcion Recepcion { get; set; } = null!;

    public virtual ICollection<SolicitudesBaja> SolicitudesBajas { get; set; } = new List<SolicitudesBaja>();
}
