using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Detalleordencompra
{
    public int Id { get; set; }

    public sbyte CantidadSolicitada { get; set; }

    public decimal PrecioUsuario { get; set; }

    public short OrdenCompraId { get; set; }

    public byte OrdenCompraProovedorId { get; set; }

    public uint OrdenCompraUsuarioId { get; set; }

    public uint OrdenCompraUsuarioRolId { get; set; }

    public int ProductoId { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
