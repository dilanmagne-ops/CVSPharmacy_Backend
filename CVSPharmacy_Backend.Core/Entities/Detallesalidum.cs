using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Detallesalidum
{
    public sbyte Id { get; set; }

    public sbyte Cantidad { get; set; }

    public short SalidasInventarioId { get; set; }

    public uint SalidasInventarioUsuarioId { get; set; }

    public uint SalidasInventarioUsuarioRolId { get; set; }

    public ushort LoteId { get; set; }

    public ushort LoteRecepcionId { get; set; }

    public short LoteRecepcionOrdenCompraId { get; set; }

    public byte LoteRecepcionOrdenCompraProovedorId { get; set; }

    public uint LoteRecepcionOrdenCompraUsuarioId { get; set; }

    public uint LoteRecepcionOrdenCompraUsuarioRolId { get; set; }

    public int LoteRecepcionOrdenCompraProductoId { get; set; }

    public uint LoteRecepcionUsuarioId { get; set; }

    public uint LoteRecepcionUsuarioRolId { get; set; }

    public int LoteProductoId { get; set; }

    public virtual Lote Lote { get; set; } = null!;

    public virtual Salidasinventario Salidasinventario { get; set; } = null!;
}
