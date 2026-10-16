using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Recepcion
{
    public ushort Id { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public string NroRemision { get; set; } = null!;

    public string? Observaciones { get; set; }

    public short OrdenCompraId { get; set; }

    public byte OrdenCompraProovedorId { get; set; }

    public uint OrdenCompraUsuarioId { get; set; }

    public uint OrdenCompraUsuarioRolId { get; set; }

    public int OrdenCompraProductoId { get; set; }

    public uint UsuarioId { get; set; }

    public uint UsuarioRolId { get; set; }

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public virtual Ordencompra Ordencompra { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
