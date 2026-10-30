using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class ProveedorController : ControllerBase
{
    private readonly IProveedorService _service;
    public ProveedorController(IProveedorService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(new ApiResponse<IReadOnlyList<ProveedorDto>>(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _service.GetByIdAsync(id);
        return proveedor is null
            ? NotFound(new { message = "Proveedor no encontrado." })
            : Ok(new ApiResponse<ProveedorDto>(proveedor));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GuardarProveedorDto dto)
    {
        var resultado = await _service.CreateAsync(dto);
        var error = ResponderError(resultado);
        if (error is not null) return error;
        return CreatedAtAction(nameof(GetById), new { id = resultado.Data!.Id },
            new ApiResponse<ProveedorDto>(resultado.Data));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GuardarProveedorDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return ResponderError(resultado) ?? Ok(new ApiResponse<ProveedorDto>(resultado.Data!));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoProveedorDto dto)
    {
        var resultado = await _service.ChangeStatusAsync(id, dto);
        return ResponderError(resultado) ?? Ok(new ApiResponse<ProveedorDto>(resultado.Data!));
    }

    private IActionResult? ResponderError(ResultadoServicio<ProveedorDto> resultado) => resultado.Estado switch
    {
        EstadoResultado.Validacion => BadRequest(new
        {
            message = resultado.Mensaje,
            errors = resultado.Errores.Select(e => new { field = e.Campo, error = e.Mensaje })
        }),
        EstadoResultado.NoEncontrado => NotFound(new { message = resultado.Mensaje }),
        EstadoResultado.Conflicto => Conflict(new { message = resultado.Mensaje }),
        _ => null
    };
}
