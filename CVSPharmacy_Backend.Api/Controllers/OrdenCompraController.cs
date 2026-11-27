using CVSPharmacy_Backend.Api.Security;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using FluentValidation;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;
using CVSPharmacy_Backend.Api.Response;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using CVSPharmacy_Backend.Core.Exceptions;

namespace CVSPharmacy_Backend.Api.Controllers
{
    [Route("api/purchase-orders")]
    [Authorize(Policy = PermisosFarmacia.PersonalFarmacia)]
    [ApiController]
    public class OrdenCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _ordenCompraService;

        private readonly IMapper _mapper;
        private readonly GuardarOrdenCompraDtoValidator _validator;

        public OrdenCompraController(
            IMapper mapper,
            GuardarOrdenCompraDtoValidator validator,
            IOrdenCompraService ordenCompraService)
        {
            _ordenCompraService = ordenCompraService;
            _mapper = mapper;
            _validator = validator;
        }
        // GET: api/purchase-orders
        [HttpGet]
        public async Task<IActionResult> GetOrdenes(
            [FromQuery] OrdenCompraQueryFilters filters)
        {
            var ordenes = await _ordenCompraService
                .GetAllOrdenesAsync(filters);

            var response =
                new ApiResponse<IEnumerable<OrdenCompraDto>>(_mapper.Map<IEnumerable<OrdenCompraDto>>(ordenes));

            return Ok(response);
        }
        // GET: api/purchase-orders/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrdenById(int id)
        {
            var orden = await _ordenCompraService
                .GetOrdenByIdAsync(id);

            var response = new ApiResponse<OrdenCompraDto>(_mapper.Map<OrdenCompraDto>(orden));

            return Ok(response);
        }
        // POST: api/purchase-orders
        [HttpPost]
        public async Task<IActionResult> InsertOrden([FromBody] GuardarOrdenCompraDto? ordenDto)
        {
            var orden = await _ordenCompraService
                .CrearOrdenAsync(await ValidarYMapearAsync(ordenDto), User.ObtenerIdUsuario());

            var response = new ApiResponse<OrdenCompraDto>(_mapper.Map<OrdenCompraDto>(orden));

            return CreatedAtAction(
                nameof(GetOrdenById),
                new { id = orden.Id },
                response);
        }

        // PUT: api/purchase-orders/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrden(int id, [FromBody] GuardarOrdenCompraDto? ordenDto)
        {
            if (id <= 0)
                throw new BusinessException("El ID de la orden debe ser mayor que cero.");
            var solicitud = await ValidarYMapearAsync(ordenDto);
            var orden = await _ordenCompraService.ActualizarOrdenAsync(id, solicitud);
            return Ok(new ApiResponse<OrdenCompraDto>(_mapper.Map<OrdenCompraDto>(orden)));
        }

        private async Task<OrdenesCompra> ValidarYMapearAsync(GuardarOrdenCompraDto? dto)
        {
            if (dto is null)
                throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
            await _validator.ValidateAndThrowAsync(dto);
            return _mapper.Map<OrdenesCompra>(dto);
        }

        // PATCH: api/purchase-orders/{id}/issue
        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id:int}/issue")]
        public async Task<IActionResult> EmitirOrden(int id)
        {
            var orden = await _ordenCompraService.EmitirOrdenAsync(id);
            return Ok(new ApiResponse<OrdenCompraDto>(_mapper.Map<OrdenCompraDto>(orden)));
        }

        // PATCH: api/purchase-orders/{id}/cancel
        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelarOrden(int id)
        {
            var orden = await _ordenCompraService.CancelarOrdenAsync(id);
            return Ok(new ApiResponse<OrdenCompraDto>(_mapper.Map<OrdenCompraDto>(orden)));
        }
    }
}
