using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public sealed class ProveedorController : ControllerBase
{
    private readonly IProveedorService _proveedorService;
    private readonly GuardarProveedorDtoValidator _validator;

    public ProveedorController(IProveedorService proveedorService, GuardarProveedorDtoValidator validator)
    {
        _proveedorService = proveedorService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proveedores = await _proveedorService.GetAllAsync();
        return Ok(new ApiResponse<IReadOnlyList<ProveedorDto>>(proveedores));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id);
        return proveedor is null
            ? NotFound(new { message = "Proveedor no encontrado." })
            : Ok(new ApiResponse<ProveedorDto>(proveedor));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GuardarProveedorDto proveedorDto)
    {
        var validation = await _validator.ValidateAsync(proveedorDto);
        if (!validation.IsValid)
            return BadRequest(new
            {
                message = "Error de validación.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                })
            });

        try
        {
            var proveedor = await _proveedorService.CreateAsync(proveedorDto);
            return CreatedAtAction(nameof(GetById), new { id = proveedor.Id },
                new ApiResponse<ProveedorDto>(proveedor));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GuardarProveedorDto proveedorDto)
    {
        var validation = await _validator.ValidateAsync(proveedorDto);
        if (!validation.IsValid)
            return BadRequest(new
            {
                message = "Error de validación.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                })
            });

        try
        {
            var proveedor = await _proveedorService.UpdateAsync(id, proveedorDto);
            return proveedor is null
                ? NotFound(new { message = "Proveedor no encontrado." })
                : Ok(new ApiResponse<ProveedorDto>(proveedor));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoProveedorDto estadoDto)
    {
        var proveedor = await _proveedorService.ChangeStatusAsync(id, estadoDto);
        return proveedor is null
            ? NotFound(new { message = "Proveedor no encontrado." })
            : Ok(new ApiResponse<ProveedorDto>(proveedor));
    }
}
