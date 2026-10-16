using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class Proovedor
{
    public byte Id { get; set; }

    public string Nit { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public string Contacto { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public sbyte Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Ordencompra> Ordencompras { get; set; } = new List<Ordencompra>();
}
