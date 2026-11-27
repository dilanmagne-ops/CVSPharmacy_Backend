using CVSPharmacy_Backend.Api.Security;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Proveedor;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[Authorize(Policy = PermisosFarmacia.PersonalFarmacia)]
[ApiController]
[Route("api/suppliers")]
public sealed class ProveedorController(IProveedorService proveedorService, IMapper mapper,
    GuardarProveedorDtoValidator guardarValidator, CambiarEstadoProveedorDtoValidator estadoValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProveedorQueryFilters filters)
    {
        var proveedores = await proveedorService.GetAllAsync(filters);
        return Ok(new ApiResponse<IReadOnlyList<ProveedorDto>>(mapper.Map<IReadOnlyList<ProveedorDto>>(proveedores)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        ValidarId(id);
        return Ok(Respuesta(await proveedorService.GetByIdAsync(id)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GuardarProveedorDto? proveedorDto)
    {
        var proveedor = await proveedorService.CreateAsync(await ValidarYMapearAsync(proveedorDto));
        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, Respuesta(proveedor));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GuardarProveedorDto? proveedorDto)
    {
        ValidarId(id);
        var cambios = await ValidarYMapearAsync(proveedorDto);
        return Ok(Respuesta(await proveedorService.UpdateAsync(id, cambios)));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CambiarEstadoProveedorDto? estadoDto)
    {
        ValidarId(id);
        if (estadoDto is null) throw CuerpoObligatorio();
        await estadoValidator.ValidateAndThrowAsync(estadoDto);
        return Ok(Respuesta(await proveedorService.ChangeStatusAsync(id, estadoDto.Activo!.Value)));
    }

    private async Task<Proveedor> ValidarYMapearAsync(GuardarProveedorDto? dto)
    {
        if (dto is null) throw CuerpoObligatorio();
        await guardarValidator.ValidateAndThrowAsync(dto);
        return mapper.Map<Proveedor>(dto);
    }

    private ApiResponse<ProveedorDto> Respuesta(Proveedor proveedor) =>
        new(mapper.Map<ProveedorDto>(proveedor));

    private static void ValidarId(int id)
    {
        if (id <= 0) throw new BusinessException("El ID del proveedor debe ser mayor que cero.");
    }

    private static BusinessException CuerpoObligatorio() =>
        new("El cuerpo de la solicitud es obligatorio.");
}
