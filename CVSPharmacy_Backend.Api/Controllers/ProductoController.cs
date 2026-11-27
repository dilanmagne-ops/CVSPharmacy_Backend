using CVSPharmacy_Backend.Api.Security;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Producto;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Producto;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[Route("api/products")]
[Authorize(Policy = PermisosFarmacia.PersonalFarmacia)]
[ApiController]
public class ProductoController(
    IProductoService productoService,
    IMapper mapper,
    GuardarProductoDtoValidator guardarValidator,
    CambiarEstadoProductoDtoValidator estadoValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductos([FromQuery] ProductoQueryFilters filters)
    {
        var productos = await productoService.GetAllProductosAsync(filters);
        return Ok(new ApiResponse<IEnumerable<ProductoDto>>(mapper.Map<IEnumerable<ProductoDto>>(productos)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductoById(int id)
    {
        var producto = await productoService.GetProductoByIdAsync(id);
        return Ok(new ApiResponse<ProductoDto>(mapper.Map<ProductoDto>(producto)));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetProductosStockCritico()
    {
        var productos = await productoService.GetProductosStockCriticoAsync();
        return Ok(new ApiResponse<IEnumerable<ProductoDto>>(mapper.Map<IEnumerable<ProductoDto>>(productos)));
    }

    [HttpPost]
    public async Task<IActionResult> InsertProducto([FromBody] GuardarProductoDto productoDto)
    {
        await guardarValidator.ValidateAndThrowAsync(productoDto);
        var producto = await productoService.CrearProductoAsync(mapper.Map<Producto>(productoDto));
        return CreatedAtAction(nameof(GetProductoById), new { id = producto.Id },
            new ApiResponse<ProductoDto>(mapper.Map<ProductoDto>(producto)));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProducto(int id, [FromBody] GuardarProductoDto productoDto)
    {
        await guardarValidator.ValidateAndThrowAsync(productoDto);
        var producto = await productoService.ActualizarProductoAsync(id, mapper.Map<Producto>(productoDto));
        return Ok(new ApiResponse<ProductoDto>(mapper.Map<ProductoDto>(producto)));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> CambiarEstadoProducto(int id, [FromBody] CambiarEstadoProductoDto estadoDto)
    {
        await estadoValidator.ValidateAndThrowAsync(estadoDto);
        var activo = await productoService.CambiarEstadoProductoAsync(id, estadoDto.Activo!.Value);
        return Ok(new ApiResponse<object>(new { message = "Estado actualizado correctamente.", id, activo }));
    }
}
