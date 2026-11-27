using AutoMapper;
using FluentValidation;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
using System.Net;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/purchase-orders/{id:int}/receptions")]
public class RecepcionController : ControllerBase
{
    private readonly IRecepcionService _recepcionService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    private readonly IMapper _mapper;
    private readonly GuardarRecepcionDtoValidator _validator;

    public RecepcionController(IRecepcionService recepcionService,
        IConfiguration configuration, IWebHostEnvironment environment, IMapper mapper, GuardarRecepcionDtoValidator validator)
    {
        _recepcionService = recepcionService;
        _configuration = configuration;
        _environment = environment;
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
        // Mismo mecanismo temporal de CU-05; no implementa sesion ni permisos por rol.
        if (!_environment.IsDevelopment())
        {
            throw new BusinessException("El usuario de pruebas solo esta disponible en Development.", HttpStatusCode.Forbidden);
        }
        var usuarioId = _configuration.GetValue<int?>("Desarrollo:UsuarioId");
        if (!usuarioId.HasValue || usuarioId.Value <= 0)
        {
            throw new BusinessException("Configure Desarrollo:UsuarioId con un usuario de pruebas valido.", HttpStatusCode.InternalServerError);
        }
        if (id <= 0) throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        if (recepcionDto is null) throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        await _validator.ValidateAndThrowAsync(recepcionDto);
        var solicitud = _mapper.Map<Recepcion>(recepcionDto);
        var recepcion = await _recepcionService.RegistrarRecepcionAsync(id, solicitud, usuarioId.Value);
        return CreatedAtAction(nameof(GetRecepciones), new { id }, new ApiResponse<RecepcionDto>(_mapper.Map<RecepcionDto>(recepcion)));
    }
}
