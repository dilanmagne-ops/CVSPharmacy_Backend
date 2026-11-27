using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using CVSPharmacy_Backend.Core.Dtos.Auth;
using Microsoft.IdentityModel.Tokens;

namespace CVSPharmacy_Backend.Api.Security;

public sealed class JwtConfig
{
    private readonly SymmetricSecurityKey _clave;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _minutos;

    public JwtConfig(IConfiguration config)
    {
        var clave = Convert.FromBase64String(config["Authentication:SecretKey"]
            ?? throw new InvalidOperationException("Configure Authentication:SecretKey en Base64."));
        if (clave.Length < 32) throw new InvalidOperationException("La clave JWT requiere al menos 32 bytes.");
        _clave = new SymmetricSecurityKey(clave);
        _issuer = config["Authentication:Issuer"] ?? "CVSPharmacy.Api";
        _audience = config["Authentication:Audience"] ?? "CVSPharmacy";
        _minutos = config.GetValue("Authentication:ExpirationMinutes", 30);
        if (_minutos <= 0) throw new InvalidOperationException("La vigencia JWT debe ser positiva.");
    }

    public TokenValidationParameters ParametrosValidacion() => new()
    {
        ValidateIssuerSigningKey = true, IssuerSigningKey = _clave,
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
        ValidateIssuer = true, ValidIssuer = _issuer,
        ValidateAudience = true, ValidAudience = _audience,
        ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
        NameClaimType = "name", RoleClaimType = "role"
    };

    public LoginResponseDto Emitir(UsuarioAutenticadoDto usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_minutos);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim("name", usuario.NombreCompleto),
            new Claim("role", usuario.Rol)
        };
        var token = new JwtSecurityToken(_issuer, _audience, claims, ahora, expira,
            new SigningCredentials(_clave, SecurityAlgorithms.HmacSha256));
        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expira, usuario);
    }
}
