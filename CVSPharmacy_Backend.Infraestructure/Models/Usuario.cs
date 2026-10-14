using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Usuario
{
    public uint Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? Email { get; set; }

    public byte[] Password { get; set; } = null!;

    public sbyte? Activo { get; set; }

    public DateTime UltimoAcceso { get; set; }

    public DateTime CreadoEn { get; set; }

    public uint RolId { get; set; }

    public virtual ICollection<Ordencompra> Ordencompras { get; set; } = new List<Ordencompra>();

    public virtual ICollection<Recepcion> Recepcions { get; set; } = new List<Recepcion>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual ICollection<Salidasinventario> Salidasinventarios { get; set; } = new List<Salidasinventario>();
}
