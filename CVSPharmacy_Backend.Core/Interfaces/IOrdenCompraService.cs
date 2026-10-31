using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.Interfaces
{
    public interface IOrdenCompraService
    {
        Task<IEnumerable<OrdenCompraDto>> 
        GetAllOrdenesAsync(
            string? status,
            int? supplierId,
            DateTime? from,
            DateTime? to);

        Task<OrdenCompraDto> GetOrdenByIdAsync(int id);

        Task<OrdenCompraDto> CrearOrdenAsync(GuardarOrdenCompraDto ordenDto, int usuarioId);
        Task<OrdenCompraDto> ActualizarOrdenAsync(int id, GuardarOrdenCompraDto ordenDto);

        Task<OrdenCompraDto> EmitirOrdenAsync(int id);

        Task<OrdenCompraDto> CancelarOrdenAsync(int id);
    }
}
