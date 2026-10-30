using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
namespace CVSPharmacy_Backend.Services.Services;

public sealed class RecepcionService(IBaseRepository<OrdenesCompra> ordenes,
    IBaseRepository<DetalleOrdenCompra> detalles, IBaseRepository<Recepcion> recepciones,
    IBaseRepository<Lote> lotes, IUnidadTrabajo unidad, IUsuarioActual usuario,
    IMapper mapper, GuardarRecepcionDtoValidator validator, TimeProvider reloj) : IRecepcionService
{
    public async Task<IReadOnlyList<RecepcionDto>> ConsultarAsync(int ordenId)
    {
        await usuario.ObtenerAsync(true);
        if (!await ordenes.AnyAsync(o => o.Id == ordenId)) throw NoEncontrada();
        return mapper.Map<IReadOnlyList<RecepcionDto>>(
            (await recepciones.GetAllAsync(r => r.OrdenId == ordenId, r => r.Lotes)).OrderBy(r => r.Id));
    }
    public async Task<RecepcionDto> RegistrarAsync(int ordenId, GuardarRecepcionDto dto)
    {
        var actor = await usuario.ObtenerAsync(true);
        await ValidacionEntrada.ValidarAsync(validator, dto);
        try
        {
            return await unidad.EjecutarAsync(async () =>
            {
                // El mismo bloqueo se usa al editar/emitir/cancelar: evita cambios incompatibles.
                var orden = await unidad.BloquearOrdenAsync(ordenId) ?? throw NoEncontrada();
                if (orden.Estado != EstadosOrden.Emitida && orden.Estado != EstadosOrden.Parcial)
                    throw new BusinessException("Solo se reciben órdenes Emitidas o Recibidas parcialmente.", HttpStatusCode.Conflict);
                var lineas = await detalles.GetAllAsync(d => d.OrdenId == ordenId);
                if (lineas.Count == 0) throw new BusinessException("La orden no tiene detalles.", HttpStatusCode.Conflict);
                var idsDetalles = lineas.Select(d => d.Id).ToList();
                var previos = await lotes.GetAllAsync(l => idsDetalles.Contains(l.DetalleOrdenId));
                var recibidos = previos.GroupBy(l => l.DetalleOrdenId)
                    .ToDictionary(g => g.Key, g => g.Sum(l => (long)l.CantidadRecibida));
                var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
                var claves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var cantidades = dto.Items.GroupBy(i => i.ProductoId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Cantidad!.Value));
                foreach (var item in dto.Items)
                {
                    if (item.FechaVencimiento!.Value <= hoy)
                        throw new BusinessException("El vencimiento debe ser posterior a hoy.", HttpStatusCode.UnprocessableEntity);
                    var numero = item.NumeroLote.Trim();
                    if (!claves.Add($"{item.ProductoId}:{numero}"))
                        throw new BusinessException("Se repite un número de lote para el mismo producto en la solicitud.", HttpStatusCode.Conflict);
                    if (await lotes.AnyAsync(l => l.ProductoId == item.ProductoId && l.NumeroLote == numero))
                        throw new BusinessException("El número de lote ya existe para este producto.", HttpStatusCode.Conflict);
                }
                foreach (var (productoId, cantidad) in cantidades)
                {
                    var coincidencias = lineas.Where(d => d.ProductoId == productoId).ToList();
                    if (coincidencias.Count == 0)
                        throw new BusinessException("Un producto recibido no pertenece a la orden.", HttpStatusCode.UnprocessableEntity);
                    if (coincidencias.Count > 1)
                        throw new BusinessException("La orden contiene productos duplicados; corrija sus detalles.", HttpStatusCode.Conflict);
                    var linea = coincidencias[0];
                    var pendiente = linea.CantidadSolicitada - recibidos.GetValueOrDefault(linea.Id);
                    if (cantidad > pendiente)
                        throw new BusinessException($"La cantidad del producto {productoId} supera lo pendiente ({pendiente}).", HttpStatusCode.UnprocessableEntity);
                }
                // Ninguna escritura se prepara hasta que todos los ítems hayan sido validados.
                var recepcion = new Recepcion
                {
                    OrdenId = ordenId, RecibidoPor = actor.Id, FechaRecepcion = reloj.GetLocalNow().DateTime,
                    NroRemision = dto.NroRemision?.Trim(), Observaciones = dto.Observaciones?.Trim()
                };
                foreach (var item in dto.Items)
                {
                    var linea = lineas.Single(d => d.ProductoId == item.ProductoId);
                    recepcion.Lotes.Add(new Lote
                    {
                        ProductoId = item.ProductoId!.Value, DetalleOrdenId = linea.Id,
                        NumeroLote = item.NumeroLote.Trim(), FechaVencimiento = item.FechaVencimiento!.Value,
                        CantidadRecibida = item.Cantidad!.Value, CantidadDisponible = item.Cantidad.Value
                    });
                }
                orden.Estado = lineas.All(d => recibidos.GetValueOrDefault(d.Id)
                    + cantidades.GetValueOrDefault(d.ProductoId) == d.CantidadSolicitada)
                    ? EstadosOrden.Recibida : EstadosOrden.Parcial;
                await recepciones.AddAsync(recepcion);
                await recepciones.SaveAsync();
                return mapper.Map<RecepcionDto>(recepcion);
            });
        }
        catch (DuplicateKeyException)
        { throw new BusinessException("Otro registro ya utiliza ese número de lote para el producto.", HttpStatusCode.Conflict); }
    }
    private static BusinessException NoEncontrada() => new("Orden de compra no encontrada.", HttpStatusCode.NotFound);
}
