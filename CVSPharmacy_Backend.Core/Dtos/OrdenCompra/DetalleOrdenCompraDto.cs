using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra
{
    public class DetalleOrdenCompraDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = null!;
        public string ProductoNombre { get; set; } = null!;
        public int CantidadSolicitada { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
