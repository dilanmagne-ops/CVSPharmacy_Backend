using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CVSPharmacy_Backend.Api.Controllers;
[ApiController]
[Route("api/purchase-orders/{id:int}/receptions")]
public sealed class RecepcionController(IRecepcionService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetRecepciones(int id) =>
        Ok(new ApiResponse<IReadOnlyList<RecepcionDto>>(await service.ConsultarAsync(id)));
    [HttpPost] public async Task<IActionResult> Registrar(int id, [FromBody] GuardarRecepcionDto dto)
    {
        var result = await service.RegistrarAsync(id, dto);
        return CreatedAtAction(nameof(GetRecepciones), new { id }, new ApiResponse<RecepcionDto>(result));
    }
}
