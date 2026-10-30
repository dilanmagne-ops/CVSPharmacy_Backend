using System;
using System.Collections.Generic;

namespace CVSPharmacy_Backend.Core.Entities;

public partial class DetalleSalida : BaseEntity
{
    public int SalidaId { get; set; }

    public int LoteId { get; set; }

    public int Cantidad { get; set; }

    public virtual Lote Lote { get; set; } = null!;

    public virtual SalidasInventario Salida { get; set; } = null!;
}
