using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos.OrdenCompra
{
    public class OrdenCompraDto
    {
        public int Id { get; set; }

        public string Numero { get; set; } = null!;

        public int ProveedorId { get; set; }

        public string ProveedorRazonSocial { get; set; } = null!;

        public int CreadoPor { get; set; }

        public string Estado { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaEmision { get; set; }

        public string? Observaciones { get; set; }

        public decimal Total { get; set; }

        public List<DetalleOrdenCompraDto> Items { get; set; } = new();
    }
}
