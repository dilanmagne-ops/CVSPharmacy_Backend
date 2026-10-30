using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CVSPharmacy_Backend.Api.Controllers;
[ApiController]
[Route("api/suppliers")]
public sealed class ProveedorController(IProveedorService service, IUsuarioActual usuario) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll()
    {
        await usuario.ObtenerAsync();
        return Ok(new ApiResponse<IReadOnlyList<ProveedorDto>>(await service.GetAllAsync()));
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id)
    {
        await usuario.ObtenerAsync();
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound(new { message = "Proveedor no encontrado." }) : Ok(new ApiResponse<ProveedorDto>(result));
    }
    [HttpPost] public async Task<IActionResult> Create([FromBody] GuardarProveedorDto dto)
    {
        await usuario.ObtenerAsync();
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<ProveedorDto>(result));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, [FromBody] GuardarProveedorDto dto)
    {
        await usuario.ObtenerAsync();
        return Ok(new ApiResponse<ProveedorDto>((await service.UpdateAsync(id, dto))!));
    }
    [HttpPatch("{id:int}/status")] public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoProveedorDto dto)
    {
        await usuario.ObtenerAsync(true);
        return Ok(new ApiResponse<ProveedorDto>((await service.ChangeStatusAsync(id, dto))!));
    }
}
