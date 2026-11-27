using System.IdentityModel.Tokens.Jwt;
using System.Net;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Api.Security;
using CVSPharmacy_Backend.Core.Dtos.Auth;
using AutoMapper;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service, LoginDtoValidator validator, IMapper mapper, JwtConfig jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto? login)
    {
        if (!ModelState.IsValid || login is null)
            throw new BusinessException("Se requiere un cuerpo JSON con email y password.");
        await validator.ValidateAndThrowAsync(login);
        var usuario = await service.AutenticarAsync(login.Email!, login.Password!);
        return Ok(new ApiResponse<LoginResponseDto>(jwt.Emitir(mapper.Map<UsuarioAutenticadoDto>(usuario))));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) || id <= 0)
            throw new BusinessException("Identidad inválida.", HttpStatusCode.Unauthorized);
        var usuario = await service.GetUsuarioActualAsync(id);
        return Ok(new ApiResponse<UsuarioAutenticadoDto>(mapper.Map<UsuarioAutenticadoDto>(usuario)));
    }
}
