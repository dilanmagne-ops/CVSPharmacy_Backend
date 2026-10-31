using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra
{
    public class ItemOrdenCompraDto
    {
        public int? ProductoId { get; set; }

        public int? CantidadSolicitada { get; set; }

        public decimal? PrecioUnitario { get; set; }
    }
}
