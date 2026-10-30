using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CVSPharmacy_Backend.Api.Controllers;
[ApiController]
[Route("api/purchase-orders")]
public sealed class OrdenCompraController(IOrdenCompraService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetOrdenes([FromQuery] string? status, [FromQuery] int? supplierId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        Ok(new ApiResponse<IEnumerable<OrdenCompraDto>>(await service.GetAllOrdenesAsync(status, supplierId, from, to)));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetOrdenById(int id) => Ok(new ApiResponse<OrdenCompraDto>(await service.GetOrdenByIdAsync(id)));
    [HttpPost] public async Task<IActionResult> InsertOrden([FromBody] GuardarOrdenCompraDto dto)
    {
        var result = await service.CrearOrdenAsync(dto);
        return CreatedAtAction(nameof(GetOrdenById), new { id = result.Id }, new ApiResponse<OrdenCompraDto>(result));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> UpdateOrden(int id, [FromBody] GuardarOrdenCompraDto dto) =>
        Ok(new ApiResponse<OrdenCompraDto>(await service.ActualizarOrdenAsync(id, dto)));
    [HttpPatch("{id:int}/issue")] public async Task<IActionResult> EmitirOrden(int id) =>
        Ok(new ApiResponse<OrdenCompraDto>(await service.EmitirOrdenAsync(id)));
    [HttpPatch("{id:int}/cancel")] public async Task<IActionResult> CancelarOrden(int id) =>
        Ok(new ApiResponse<OrdenCompraDto>(await service.CancelarOrdenAsync(id)));
}
