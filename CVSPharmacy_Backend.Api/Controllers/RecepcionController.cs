using CVSPharmacy_Backend.Api.Security;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using FluentValidation;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[Authorize(Policy = PermisosFarmacia.PersonalFarmacia, Roles = "Administrador")]
[ApiController]
[Route("api/purchase-orders/{id:int}/receptions")]
public class RecepcionController : ControllerBase
{
    private readonly IRecepcionService _recepcionService;

    private readonly IMapper _mapper;
    private readonly GuardarRecepcionDtoValidator _validator;

    public RecepcionController(IRecepcionService recepcionService,
        IMapper mapper, GuardarRecepcionDtoValidator validator)
    {
        _recepcionService = recepcionService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecepciones(int id,
        [FromQuery] RecepcionQueryFilters filters)
    {
        var recepciones = await _recepcionService.GetRecepcionesAsync(id, filters);
        return Ok(new ApiResponse<IEnumerable<RecepcionDto>>(_mapper.Map<IEnumerable<RecepcionDto>>(recepciones)));
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarRecepcion(int id, [FromBody] GuardarRecepcionDto? recepcionDto)
    {
        if (id <= 0) throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        if (recepcionDto is null) throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        await _validator.ValidateAndThrowAsync(recepcionDto);
        var solicitud = _mapper.Map<Recepcion>(recepcionDto);
        var recepcion = await _recepcionService.RegistrarRecepcionAsync(id, solicitud, User.ObtenerIdUsuario());
        return CreatedAtAction(nameof(GetRecepciones), new { id }, new ApiResponse<RecepcionDto>(_mapper.Map<RecepcionDto>(recepcion)));
    }
}
