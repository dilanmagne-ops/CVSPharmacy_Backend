using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class SolicitudesBaja : BaseEntity
{
    public int LoteId { get; set; }

    public int Cantidad { get; set; }

    public string Estado { get; set; } = null!;

    public int SolicitadoPor { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public string? Observacion { get; set; }

    public int? ResueltoPor { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? MotivoResolucion { get; set; }

    public int? SalidaId { get; set; }

    public virtual Lote Lote { get; set; } = null!;

    public virtual Usuario? ResueltoPorNavigation { get; set; }

    public virtual SalidasInventario? Salida { get; set; }

    public virtual Usuario SolicitadoPorNavigation { get; set; } = null!;
}
