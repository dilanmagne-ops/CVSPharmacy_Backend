using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Lote
{
    public ushort Id { get; set; }

    public string NumeroLote { get; set; } = null!;

    public DateTime? FechaVencimiento { get; set; }

    public string? CantidadRecibida { get; set; }

    public ushort RecepcionId { get; set; }

    public short RecepcionOrdenCompraId { get; set; }

    public byte RecepcionOrdenCompraProovedorId { get; set; }

    public uint RecepcionOrdenCompraUsuarioId { get; set; }

    public uint RecepcionOrdenCompraUsuarioRolId { get; set; }

    public int RecepcionOrdenCompraProductoId { get; set; }

    public uint RecepcionUsuarioId { get; set; }

    public uint RecepcionUsuarioRolId { get; set; }

    public int ProductoId { get; set; }

    public virtual ICollection<Detallesalidum> Detallesalida { get; set; } = new List<Detallesalidum>();

    public virtual Producto Producto { get; set; } = null!;

    public virtual Recepcion Recepcion { get; set; } = null!;
}
