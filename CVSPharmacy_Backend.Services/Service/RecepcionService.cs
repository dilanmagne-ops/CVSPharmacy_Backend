using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Recepcion;

namespace CVSPharmacy_Backend.Services.Service;

public class RecepcionService : IRecepcionService
{
    private readonly IRecepcionRepository _recepcionRepository;
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly GuardarRecepcionDtoValidator _validator;

    public RecepcionService(IRecepcionRepository recepcionRepository,
        IOrdenCompraRepository ordenCompraRepository, IMapper mapper,
        GuardarRecepcionDtoValidator validator)
    {
        _recepcionRepository = recepcionRepository;
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IEnumerable<RecepcionDto>> GetRecepcionesAsync(
        int ordenId, DateTime? from = null, DateTime? to = null)
    {
        if (ordenId <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
        {
            throw new BusinessException("La fecha inicial no puede ser posterior a la fecha final.");
        }
        var orden = await _ordenCompraRepository.GetById(ordenId);
        if (orden is null)
        {
            throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
        }
        var recepciones = await _recepcionRepository.GetPorOrdenAsync(ordenId);
        if (from.HasValue)
        {
            recepciones = recepciones.Where(recepcion => recepcion.FechaRecepcion.Date >= from.Value.Date);
        }
        if (to.HasValue)
        {
            recepciones = recepciones.Where(recepcion => recepcion.FechaRecepcion.Date <= to.Value.Date);
        }
        return _mapper.Map<IEnumerable<RecepcionDto>>(recepciones);
    }

    public async Task<RecepcionDto> RegistrarRecepcionAsync(
        int ordenId, GuardarRecepcionDto recepcionDto, int usuarioId)
    {
        if (ordenId <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        if (recepcionDto is null)
        {
            throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        }
        var validacion = await _validator.ValidateAsync(recepcionDto);
        if (!validacion.IsValid)
        {
            throw new BusinessException("Error de validacion.", HttpStatusCode.BadRequest,
                validacion.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }
        if (usuarioId <= 0)
        {
            throw new BusinessException("Configure un usuario de pruebas valido.", HttpStatusCode.InternalServerError);
        }
        var usuario = await _ordenCompraRepository.GetUsuarioAsync(usuarioId);
        if (usuario is null || usuario.Activo != true)
        {
            throw new BusinessException("El usuario de pruebas no existe o esta inactivo.", HttpStatusCode.Forbidden);
        }

        try
        {
            return await _ordenCompraRepository.EjecutarEnTransaccionAsync(async () =>
            {
                // Mismo bloqueo de CU-05: coordina recepcion, edicion y cancelacion.
                var orden = await _ordenCompraRepository.GetOrdenParaActualizarAsync(ordenId);
                if (orden is null)
                {
                    throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
                }
                if (orden.Estado != "Emitida" && orden.Estado != "Recibida parcial")
                {
                    throw new BusinessException("Solo se reciben ordenes Emitidas o Recibidas parcialmente.", HttpStatusCode.Conflict);
                }
                var detalles = orden.DetalleOrdenCompras.ToList();
                if (detalles.Count == 0 || detalles.Any(detalle => detalle.CantidadSolicitada <= 0) ||
                    detalles.Select(detalle => detalle.ProductoId).Distinct().Count() != detalles.Count)
                {
                    throw new BusinessException("Los detalles de la orden no son validos para recibir mercaderia.", HttpStatusCode.Conflict);
                }
                var lotesAnteriores = await _recepcionRepository.GetLotesPorOrdenAsync(ordenId);
                // CantidadRecibida es historica; CantidadDisponible puede disminuir por salidas.
                var recibidos = lotesAnteriores.GroupBy(lote => lote.DetalleOrdenId)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(lote => (long)lote.CantidadRecibida));
                if (detalles.Any(detalle => recibidos.GetValueOrDefault(detalle.Id) > detalle.CantidadSolicitada))
                {
                    throw new BusinessException("La orden tiene cantidades recibidas inconsistentes.", HttpStatusCode.Conflict);
                }
                var ahora = DateTime.Now;
                var hoy = DateOnly.FromDateTime(ahora);
                var claves = new HashSet<(int ProductoId, string NumeroLote)>();
                var cantidades = recepcionDto.Items.GroupBy(item => item.ProductoId!.Value)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => (long)item.Cantidad!.Value));

                foreach (var item in recepcionDto.Items)
                {
                    if (!detalles.Any(detalle => detalle.ProductoId == item.ProductoId!.Value))
                    {
                        throw new BusinessException("Un producto recibido no pertenece a la orden.", HttpStatusCode.UnprocessableEntity);
                    }
                    if (item.FechaVencimiento!.Value <= hoy)
                    {
                        throw new BusinessException("La fecha de vencimiento debe ser posterior a hoy.", HttpStatusCode.UnprocessableEntity);
                    }
                    var numeroLote = item.NumeroLote.Trim();
                    if (!claves.Add((item.ProductoId!.Value, numeroLote.ToUpperInvariant())))
                    {
                        throw new BusinessException("Se repite un lote para el mismo producto en la solicitud.", HttpStatusCode.Conflict);
                    }
                    if (await _recepcionRepository.ExisteLoteAsync(item.ProductoId.Value, numeroLote))
                    {
                        throw new BusinessException("El numero de lote ya existe para el producto.", HttpStatusCode.Conflict);
                    }
                }

                foreach (var cantidad in cantidades)
                {
                    var detalle = detalles.Single(detalle => detalle.ProductoId == cantidad.Key);
                    var pendiente = detalle.CantidadSolicitada - recibidos.GetValueOrDefault(detalle.Id);
                    if (cantidad.Value > pendiente)
                    {
                        throw new BusinessException(
                            $"La cantidad del producto {cantidad.Key} supera lo pendiente ({pendiente}).",
                            HttpStatusCode.UnprocessableEntity);
                    }
                }

                // Preparar escrituras solo despues de validar todos los lotes y cantidades.
                var recepcion = new Recepcion
                {
                    OrdenId = ordenId,
                    RecibidoPor = usuario.Id,
                    FechaRecepcion = ahora,
                    NroRemision = recepcionDto.NroRemision?.Trim(),
                    Observaciones = recepcionDto.Observaciones?.Trim()
                };
                foreach (var item in recepcionDto.Items)
                {
                    var detalle = detalles.Single(detalle => detalle.ProductoId == item.ProductoId!.Value);
                    recepcion.Lotes.Add(new Lote
                    {
                        ProductoId = item.ProductoId!.Value,
                        DetalleOrdenId = detalle.Id,
                        NumeroLote = item.NumeroLote.Trim(),
                        FechaVencimiento = item.FechaVencimiento!.Value,
                        CantidadRecibida = item.Cantidad!.Value,
                        CantidadDisponible = item.Cantidad.Value
                    });
                }
                orden.Estado = detalles.All(detalle =>
                    recibidos.GetValueOrDefault(detalle.Id) + cantidades.GetValueOrDefault(detalle.ProductoId)
                    == detalle.CantidadSolicitada) ? "Recibida" : "Recibida parcial";

                await _recepcionRepository.Add(recepcion);
                return _mapper.Map<RecepcionDto>(recepcion);
            });
        }
        catch (NumeroLoteDuplicadoException)
        {
            // Incluye colisiones concurrentes y equivalencias de la intercalacion MySQL.
            throw new BusinessException("El numero de lote ya existe para el producto.", HttpStatusCode.Conflict);
        }
    }
}
