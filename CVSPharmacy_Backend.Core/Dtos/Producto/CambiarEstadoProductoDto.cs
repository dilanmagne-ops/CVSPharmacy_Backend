namespace CVSPharmacy_Backend.Core.Dtos.Producto
{
    //PATCH — Activación o desactivación de productos.
    public class CambiarEstadoProductoDto
    {
        public bool? Activo { get; set; }
    }
}
