using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class SalidasInventario
{
    public int Id { get; set; }

    public DateTime FechaSalida { get; set; }

    public string Motivo { get; set; } = null!;

    public int UsuarioId { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<DetalleSalida> DetalleSalida { get; set; } = new List<DetalleSalida>();

    public virtual SolicitudesBaja? SolicitudesBaja { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
