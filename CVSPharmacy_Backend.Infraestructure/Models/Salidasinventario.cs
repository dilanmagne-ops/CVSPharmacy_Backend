using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Salidasinventario
{
    public short Id { get; set; }

    public DateTime FechaSalida { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }

    public uint UsuarioId { get; set; }

    public uint UsuarioRolId { get; set; }

    public virtual ICollection<Detallesalidum> Detallesalida { get; set; } = new List<Detallesalidum>();

    public virtual Usuario Usuario { get; set; } = null!;
}
