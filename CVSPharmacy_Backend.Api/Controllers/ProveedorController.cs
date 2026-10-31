using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class ProveedorController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedorController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? q = null, [FromQuery] bool? active = null)
    {
        var proveedores = await _proveedorService.GetAllAsync(q, active);
        return Ok(new ApiResponse<IReadOnlyList<ProveedorDto>>(proveedores));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id);
        return Ok(new ApiResponse<ProveedorDto>(proveedor));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GuardarProveedorDto proveedorDto)
    {
        var proveedor = await _proveedorService.CreateAsync(proveedorDto);
        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id },
            new ApiResponse<ProveedorDto>(proveedor));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GuardarProveedorDto proveedorDto)
    {
        var proveedor = await _proveedorService.UpdateAsync(id, proveedorDto);
        return Ok(new ApiResponse<ProveedorDto>(proveedor));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoProveedorDto estadoDto)
    {
        var proveedor = await _proveedorService.ChangeStatusAsync(id, estadoDto);
        return Ok(new ApiResponse<ProveedorDto>(proveedor));
    }
}
