using AutoMapper;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Api.Security;
using CVSPharmacy_Backend.Core.Dtos.Reporteria;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Policy = PermisosFarmacia.PersonalFarmacia, Roles = "Administrador")]
public sealed class ReportesController(IReportesService reportesService, IMapper mapper) : ControllerBase
{
    [HttpGet("dapper/ranking-productos-por-proveedor")]
    public async Task<IActionResult> GetRankingProductosPorProveedor()
    {
        var ranking = await reportesService.GetRankingProductosPorProveedorAsync();
        var resultado = mapper.Map<IEnumerable<RankingProductoProveedorDto>>(ranking);
        return Ok(new ApiResponse<IEnumerable<RankingProductoProveedorDto>>(resultado));
    }
}
