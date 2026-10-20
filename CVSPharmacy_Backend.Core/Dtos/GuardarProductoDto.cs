using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Dtos
{
    //POST y PUT — Datos para registrar o actualizar productos.
    public class GuardarProductoDto
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? PrincipioActivo { get; set; }
        public string Presentacion { get; set; } = null!;
        public string UnidadMedida { get; set; } = null!;
        public decimal PrecioVenta { get; set; }
        public int StockMinimo { get; set; }
    }
}
