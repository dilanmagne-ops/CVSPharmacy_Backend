using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using CVSPharmacy_Backend.Core.Exceptions;

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/purchase-orders")]
    [ApiController]
    public class OrdenCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _ordenCompraService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public OrdenCompraController(
            IOrdenCompraService ordenCompraService,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _ordenCompraService = ordenCompraService;
            _configuration = configuration;
            _environment = environment;
        }
        // GET: api/purchase-orders
        [HttpGet]
        public async Task<IActionResult> GetOrdenes(
            [FromQuery] string? status,
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var ordenes = await _ordenCompraService
                .GetAllOrdenesAsync(status, supplierId, from, to);

            var response =
                new ApiResponse<IEnumerable<OrdenCompraDto>>(ordenes);

            return Ok(response);
        }
        // GET: api/purchase-orders/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrdenById(int id)
        {
            var orden = await _ordenCompraService
                .GetOrdenByIdAsync(id);

            var response = new ApiResponse<OrdenCompraDto>(orden);

            return Ok(response);
        }
        // POST: api/purchase-orders
        [HttpPost]
        public async Task<IActionResult> InsertOrden([FromBody] GuardarOrdenCompraDto ordenDto)
        {
            // Responsable temporal para pruebas, sin implementar login.
            if (!_environment.IsDevelopment())
            {
                throw new BusinessException(
                    "El usuario de pruebas solo esta disponible en Development.",
                    HttpStatusCode.Forbidden);
            }

            var usuarioId = _configuration
                .GetValue<int?>("Desarrollo:UsuarioId");

            if (!usuarioId.HasValue || usuarioId.Value <= 0)
            {
                throw new BusinessException(
                    "Configure Desarrollo:UsuarioId con un usuario de pruebas valido.",
                    HttpStatusCode.InternalServerError);
            }

            var orden = await _ordenCompraService
                .CrearOrdenAsync(ordenDto, usuarioId.Value);

            var response = new ApiResponse<OrdenCompraDto>(orden);

            return CreatedAtAction(
                nameof(GetOrdenById),
                new { id = orden.Id },
                response);
        }

        // PUT: api/purchase-orders/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrden(int id, [FromBody] GuardarOrdenCompraDto ordenDto)
        {
            var orden = await _ordenCompraService.ActualizarOrdenAsync(id, ordenDto);
            return Ok(new ApiResponse<OrdenCompraDto>(orden));
        }

        // PATCH: api/purchase-orders/{id}/issue
        [HttpPatch("{id:int}/issue")]
        public async Task<IActionResult> EmitirOrden(int id)
        {
            var orden = await _ordenCompraService.EmitirOrdenAsync(id);
            return Ok(new ApiResponse<OrdenCompraDto>(orden));
        }

        // PATCH: api/purchase-orders/{id}/cancel
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelarOrden(int id)
        {
            var orden = await _ordenCompraService.CancelarOrdenAsync(id);
            return Ok(new ApiResponse<OrdenCompraDto>(orden));
        }
    }
}
