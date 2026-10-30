using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Core.Results;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;

namespace CVSPharmacy_Backend.Services.Service
{
    public class OrdenCompraService : IOrdenCompraService
    {
        // TODO: reemplazar por el id del usuario autenticado cuando exista CU-01.
        // Debe ser un usuario que exista en la tabla usuarios.
        private const int UsuarioTemporalId = 1;

        private const int MaxIntentosNumero = 3;

        private const string Borrador = "Borrador";
        private const string Emitida = "Emitida";
        private const string Cancelada = "Cancelada";

        private readonly IOrdenCompraRepository _ordenRepository;
        private readonly IMapper _mapper;
        private readonly GuardarOrdenCompraDtoValidator _validator;

        public OrdenCompraService(
            IOrdenCompraRepository ordenRepository,
            IMapper mapper,
            GuardarOrdenCompraDtoValidator validator)
        {
            _ordenRepository = ordenRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<OrdenCompraDto>> GetAllOrdenesAsync(
            string? estado, int? proveedorId, DateTime? desde, DateTime? hasta)
        {
            var ordenes = await _ordenRepository.GetAllOrdenesAsync(estado, proveedorId, desde, hasta);
            return _mapper.Map<IEnumerable<OrdenCompraDto>>(ordenes);
        }

        public async Task<OrdenCompraDto?> GetOrdenByIdAsync(int id)
        {
            var orden = await _ordenRepository.GetOrdenByIdAsync(id);
            if (orden == null)
                return null;

            return _mapper.Map<OrdenCompraDto>(orden);
        }

        // ---------- Validaciones ----------

        // Formato: responde 400
        private async Task<ResultadoServicio<OrdenCompraDto>?> ValidarFormatoAsync(GuardarOrdenCompraDto ordenDto)
        {
            var resultado = await _validator.ValidateAsync(ordenDto);

            if (resultado.IsValid)
                return null;

            return new ResultadoServicio<OrdenCompraDto>
            {
                Estado = EstadoResultado.Validacion,
                Mensaje = "Error de validacion.",
                Errores = resultado.Errors.Select(e => new ErrorCampo
                {
                    Campo = e.PropertyName,
                    Mensaje = e.ErrorMessage
                }).ToList()
            };
        }

        // RN-10: proveedor activo y productos activos. Responde 422
        private async Task<ResultadoServicio<OrdenCompraDto>?> ValidarReglasNegocioAsync(GuardarOrdenCompraDto ordenDto)
        {
            var proveedor = await _ordenRepository.GetProveedorAsync(ordenDto.ProveedorId!.Value);
            if (proveedor == null || proveedor.Activo != true)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.NoProcesable,
                    Mensaje = "El proveedor no existe o está inactivo."
                };
            }

            var idsSolicitados = ordenDto.Items.Select(i => i.ProductoId!.Value).Distinct().ToList();
            var productos = await _ordenRepository.GetProductosAsync(idsSolicitados);

            var invalidos = idsSolicitados
                .Where(id => !productos.Any(p => p.Id == id && p.Activo == true))
                .ToList();

            if (invalidos.Count > 0)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.NoProcesable,
                    Mensaje = $"Productos inexistentes o inactivos: {string.Join(", ", invalidos)}."
                };
            }

            return null;
        }

        // ---------- POST ----------

        public async Task<ResultadoServicio<OrdenCompraDto>> CrearOrdenAsync(GuardarOrdenCompraDto ordenDto)
        {
            var formato = await ValidarFormatoAsync(ordenDto);
            if (formato != null)
                return formato;

            var reglas = await ValidarReglasNegocioAsync(ordenDto);
            if (reglas != null)
                return reglas;

            // Si dos usuarios piden el mismo número a la vez, se reintenta con el siguiente
            for (var intento = 1; intento <= MaxIntentosNumero; intento++)
            {
                var orden = new OrdenesCompra
                {
                    Numero = await _ordenRepository.GenerarNumeroAsync(),
                    ProveedorId = ordenDto.ProveedorId!.Value,
                    CreadoPor = UsuarioTemporalId,
                    Estado = Borrador,
                    FechaCreacion = DateTime.Now,
                    Observaciones = ordenDto.Observaciones?.Trim(),
                    DetalleOrdenCompras = _mapper.Map<List<DetalleOrdenCompra>>(ordenDto.Items)
                };

                try
                {
                    await _ordenRepository.InsertOrdenAsync(orden);

                    var guardada = await _ordenRepository.GetOrdenByIdAsync(orden.Id);
                    return new ResultadoServicio<OrdenCompraDto>
                    {
                        Estado = EstadoResultado.Exito,
                        Data = _mapper.Map<OrdenCompraDto>(guardada!)
                    };
                }
                catch (NumeroOrdenDuplicadoException)
                {
                    // otro usuario tomó ese número; se vuelve a intentar
                }
            }

            return new ResultadoServicio<OrdenCompraDto>
            {
                Estado = EstadoResultado.Conflicto,
                Mensaje = "No se pudo generar un número de orden único. Intente nuevamente."
            };
        }

        // ---------- PUT ----------

        public async Task<ResultadoServicio<OrdenCompraDto>> ActualizarOrdenAsync(int id, GuardarOrdenCompraDto ordenDto)
        {
            var formato = await ValidarFormatoAsync(ordenDto);
            if (formato != null)
                return formato;

            var orden = await _ordenRepository.GetOrdenParaActualizarAsync(id);
            if (orden == null)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.NoEncontrado,
                    Mensaje = "Orden de compra no encontrada."
                };
            }

            // RN-11: solo se modifican órdenes en Borrador
            if (orden.Estado != Borrador)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = $"Solo se pueden editar órdenes en Borrador. Estado actual: {orden.Estado}."
                };
            }

            var reglas = await ValidarReglasNegocioAsync(ordenDto);
            if (reglas != null)
                return reglas;

            orden.ProveedorId = ordenDto.ProveedorId!.Value;
            orden.Observaciones = ordenDto.Observaciones?.Trim();
            _ordenRepository.ReemplazarDetalles(orden, _mapper.Map<List<DetalleOrdenCompra>>(ordenDto.Items));

            await _ordenRepository.SaveAsync();

            var actualizada = await _ordenRepository.GetOrdenByIdAsync(id);
            return new ResultadoServicio<OrdenCompraDto>
            {
                Estado = EstadoResultado.Exito,
                Data = _mapper.Map<OrdenCompraDto>(actualizada!)
            };
        }

        // ---------- PATCH issue ----------

        public async Task<ResultadoServicio<OrdenCompraDto>> EmitirOrdenAsync(int id)
        {
            var orden = await _ordenRepository.GetOrdenParaActualizarAsync(id);
            if (orden == null)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.NoEncontrado,
                    Mensaje = "Orden de compra no encontrada."
                };
            }

            // RN-11: solo Borrador -> Emitida
            if (orden.Estado != Borrador)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = $"Solo se puede emitir una orden en Borrador. Estado actual: {orden.Estado}."
                };
            }

            orden.Estado = Emitida;
            orden.FechaEmision = DateTime.Now;
            await _ordenRepository.SaveAsync();

            var emitida = await _ordenRepository.GetOrdenByIdAsync(id);
            return new ResultadoServicio<OrdenCompraDto>
            {
                Estado = EstadoResultado.Exito,
                Data = _mapper.Map<OrdenCompraDto>(emitida!)
            };
        }

        // ---------- PATCH cancel ----------

        public async Task<ResultadoServicio<OrdenCompraDto>> CancelarOrdenAsync(int id)
        {
            var orden = await _ordenRepository.GetOrdenParaActualizarAsync(id);
            if (orden == null)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.NoEncontrado,
                    Mensaje = "Orden de compra no encontrada."
                };
            }

            // RN-11: solo se cancela en Borrador o Emitida
            if (orden.Estado != Borrador && orden.Estado != Emitida)
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = $"No se puede cancelar una orden en estado {orden.Estado}."
                };
            }

            // RN-11: ... y solo si no tiene recepciones
            if (await _ordenRepository.TieneRecepcionesAsync(id))
            {
                return new ResultadoServicio<OrdenCompraDto>
                {
                    Estado = EstadoResultado.Conflicto,
                    Mensaje = "No se puede cancelar una orden que ya tiene recepciones registradas."
                };
            }

            orden.Estado = Cancelada;
            await _ordenRepository.SaveAsync();

            var cancelada = await _ordenRepository.GetOrdenByIdAsync(id);
            return new ResultadoServicio<OrdenCompraDto>
            {
                Estado = EstadoResultado.Exito,
                Data = _mapper.Map<OrdenCompraDto>(cancelada!)
            };
        }
    }
}
