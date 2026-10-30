using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/purchase-orders")]
    [ApiController]
    public class OrdenCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _ordenService;

        public OrdenCompraController(IOrdenCompraService ordenService)
        {
            _ordenService = ordenService;
        }

        private IActionResult? ResponderError<T>(ResultadoServicio<T> resultado)
        {
            switch (resultado.Estado)
            {
                case EstadoResultado.Validacion:
                    return BadRequest(new
                    {
                        message = resultado.Mensaje,
                        errors = resultado.Errores.Select(e => new
                        {
                            field = e.Campo,
                            error = e.Mensaje
                        })
                    });

                case EstadoResultado.NoEncontrado:
                    return NotFound(new { message = resultado.Mensaje });

                case EstadoResultado.Conflicto:
                    return Conflict(new { message = resultado.Mensaje });

                case EstadoResultado.NoProcesable:
                    return UnprocessableEntity(new { message = resultado.Mensaje });

                default:
                    return null;
            }
        }

        #region Endpoints CU-05

        // GET: api/purchase-orders?status=Borrador&supplierId=1&from=2026-10-01&to=2026-10-31
        [HttpGet]
        public async Task<IActionResult> GetOrdenes(
            [FromQuery] string? status,
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var ordenesDto = await _ordenService.GetAllOrdenesAsync(status, supplierId, from, to);
            return Ok(new ApiResponse<IEnumerable<OrdenCompraDto>>(ordenesDto));
        }

        // GET: api/purchase-orders/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrdenById(int id)
        {
            var ordenDto = await _ordenService.GetOrdenByIdAsync(id);
            if (ordenDto == null)
            {
                return NotFound(new { message = "Orden de compra no encontrada." });
            }

            return Ok(new ApiResponse<OrdenCompraDto>(ordenDto));
        }

        // POST: api/purchase-orders
        [HttpPost]
        public async Task<IActionResult> InsertOrden([FromBody] GuardarOrdenCompraDto ordenDto)
        {
            var resultado = await _ordenService.CrearOrdenAsync(ordenDto);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }

            var response = new ApiResponse<OrdenCompraDto>(resultado.Data!);
            return CreatedAtAction(
                nameof(GetOrdenById),
                new { id = resultado.Data!.Id },
                response);
        }

        // PUT: api/purchase-orders/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrden(int id, [FromBody] GuardarOrdenCompraDto ordenDto)
        {
            var resultado = await _ordenService.ActualizarOrdenAsync(id, ordenDto);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }

            return Ok(new ApiResponse<OrdenCompraDto>(resultado.Data!));
        }

        // PATCH: api/purchase-orders/5/issue
        [HttpPatch("{id:int}/issue")]
        public async Task<IActionResult> EmitirOrden(int id)
        {
            var resultado = await _ordenService.EmitirOrdenAsync(id);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }

            return Ok(new ApiResponse<OrdenCompraDto>(resultado.Data!));
        }

        // PATCH: api/purchase-orders/5/cancel
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelarOrden(int id)
        {
            var resultado = await _ordenService.CancelarOrdenAsync(id);
            var error = ResponderError(resultado);
            if (error != null)
            {
                return error;
            }

            return Ok(new ApiResponse<OrdenCompraDto>(resultado.Data!));
        }

        #endregion
    }
}
