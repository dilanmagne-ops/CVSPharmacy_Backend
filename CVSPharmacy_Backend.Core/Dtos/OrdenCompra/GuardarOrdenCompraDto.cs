using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra
{
    public class GuardarOrdenCompraDto
    {
        public int? ProveedorId { get; set; }

        public string? Observaciones { get; set; }

        public List<ItemOrdenCompraDto> Items { get; set; } = new();
    }
}
