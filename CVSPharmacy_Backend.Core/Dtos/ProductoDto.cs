using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos
{
    //GET — Información del producto, incluyendo stock disponible y stock crítico.
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? PrincipioActivo { get; set; }
        public string Presentacion { get; set; } = null!;
        public string UnidadMedida { get; set; } = null!;
        public decimal PrecioVenta { get; set; }
        public int StockMinimo { get; set; }
        public bool? Activo { get; set; }
        public DateTime CreadoEn { get; set; }
        public int StockDisponible { get; set; }
        public bool EsStockCritico { get; set; }
    }
}
