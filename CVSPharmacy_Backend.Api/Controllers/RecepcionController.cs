using System.Net;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/purchase-orders/{id:int}/receptions")]
public class RecepcionController : ControllerBase
{
    private readonly IRecepcionService _recepcionService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public RecepcionController(IRecepcionService recepcionService,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        _recepcionService = recepcionService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecepciones(int id,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var recepciones = await _recepcionService.GetRecepcionesAsync(id, from, to);
        return Ok(new ApiResponse<IEnumerable<RecepcionDto>>(recepciones));
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarRecepcion(int id, [FromBody] GuardarRecepcionDto recepcionDto)
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
        var recepcion = await _recepcionService.RegistrarRecepcionAsync(id, recepcionDto, usuarioId.Value);
        return CreatedAtAction(nameof(GetRecepciones), new { id }, new ApiResponse<RecepcionDto>(recepcion));
    }
}
