using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RolId { get; set; }

    public bool? Activo { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<OrdenesCompra> OrdenesCompras { get; set; } = new List<OrdenesCompra>();

    public virtual ICollection<Recepcion> Recepciones { get; set; } = new List<Recepcion>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual ICollection<SalidasInventario> SalidasInventarios { get; set; } = new List<SalidasInventario>();

    public virtual ICollection<SolicitudesBaja> SolicitudesBajaResueltoPorNavigations { get; set; } = new List<SolicitudesBaja>();

    public virtual ICollection<SolicitudesBaja> SolicitudesBajaSolicitadoPorNavigations { get; set; } = new List<SolicitudesBaja>();
}
