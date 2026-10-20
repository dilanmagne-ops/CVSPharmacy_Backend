using System.Text.Json.Serialization;

namespace CVSPharmacy_Backend.Core.Dtos
{
    //PATCH — Activación o desactivación de productos.
    public class CambiarEstadoProductoDto
    {
        [JsonRequired]
        public bool Activo { get; set; }
    }
}
