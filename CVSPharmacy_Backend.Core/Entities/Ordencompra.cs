using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Ordencompra
{
    public short Id { get; set; }

    public string Numero { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaEmision { get; set; }

    public string? Observaciones { get; set; }

    public byte ProovedorId { get; set; }

    public uint UsuarioId { get; set; }

    public uint UsuarioRolId { get; set; }

    public int ProductoId { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Proovedor Proovedor { get; set; } = null!;

    public virtual ICollection<Recepcion> Recepcions { get; set; } = new List<Recepcion>();

    public virtual Usuario Usuario { get; set; } = null!;
}
