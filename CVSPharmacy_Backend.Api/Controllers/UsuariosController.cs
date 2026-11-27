using System.IdentityModel.Tokens.Jwt;
using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.Usuario;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Usuario;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/users")]
public sealed class UsuariosController(IUsuarioService usuarios, IMapper mapper,
    CrearUsuarioValidator crearValidator, EditarUsuarioValidator editarValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? rol, [FromQuery] bool? activo)
    {
        ValidarSolicitud();
        var lista = await usuarios.ListarAsync(ActorId(), rol, activo);
        return Ok(new ApiResponse<IReadOnlyList<UsuarioDto>>(mapper.Map<IReadOnlyList<UsuarioDto>>(lista)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id) =>
        Ok(Respuesta(await usuarios.ObtenerAsync(ActorId(), id)));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto? dto)
    {
        ValidarSolicitud();
        if (dto is null) throw new BusinessException("El cuerpo es obligatorio.");
        await crearValidator.ValidateAndThrowAsync(dto);
        var usuario = await usuarios.CrearAsync(ActorId(), dto);
        return CreatedAtAction(nameof(Obtener), new { id = usuario.Id }, Respuesta(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarUsuarioDto? dto)
    {
        ValidarSolicitud();
        if (dto is null) throw new BusinessException("El cuerpo es obligatorio.");
        await editarValidator.ValidateAndThrowAsync(dto);
        return Ok(Respuesta(await usuarios.EditarAsync(ActorId(), id, dto)));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoUsuarioDto? dto)
    {
        ValidarSolicitud();
        if (dto?.Activo is not bool activo) throw new BusinessException("Indique el estado activo.");
        return Ok(Respuesta(await usuarios.CambiarEstadoAsync(ActorId(), id, activo)));
    }

    private int ActorId() => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
        && id > 0 ? id : throw new BusinessException("Identidad inválida.", HttpStatusCode.Unauthorized);

    private void ValidarSolicitud()
    {
        if (!ModelState.IsValid) throw new BusinessException("Revise los campos de la solicitud.");
    }

    private ApiResponse<UsuarioDto> Respuesta(Usuario usuario) => new(mapper.Map<UsuarioDto>(usuario));
}
