using System.Net;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public class RecepcionService(IUnitOfWork unitOfWork) : IRecepcionService
{
    public async Task<IEnumerable<Recepcion>> GetRecepcionesAsync(
        int ordenId, RecepcionQueryFilters filters)
    {
        if (ordenId <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        var from = filters.From;
        var to = filters.To;
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
        {
            throw new BusinessException("La fecha inicial no puede ser posterior a la fecha final.");
        }
        var orden = await unitOfWork.OrdenCompraRepository.GetById(ordenId);
        if (orden is null)
        {
            throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
        }
        var recepciones = await unitOfWork.RecepcionRepository.GetPorOrdenAsync(ordenId);
        if (from.HasValue)
        {
            recepciones = recepciones.Where(recepcion => recepcion.FechaRecepcion.Date >= from.Value.Date);
        }
        if (to.HasValue)
        {
            recepciones = recepciones.Where(recepcion => recepcion.FechaRecepcion.Date <= to.Value.Date);
        }
        return recepciones;
    }

    public async Task<Recepcion> RegistrarRecepcionAsync(
        int ordenId, Recepcion recepcion, int usuarioId)
    {
        if (ordenId <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        if (usuarioId <= 0)
        {
            throw new BusinessException("Configure un usuario de pruebas valido.", HttpStatusCode.InternalServerError);
        }
        var usuario = await unitOfWork.UsuarioRepository.GetById(usuarioId);
        if (usuario is null || usuario.Activo != true)
        {
            throw new BusinessException("El usuario de pruebas no existe o esta inactivo.", HttpStatusCode.Forbidden);
        }

        try
        {
            return await unitOfWork.EjecutarEnTransaccionAsync(async () =>
            {
                // Mismo bloqueo de CU-05: coordina recepcion, edicion y cancelacion.
                var orden = await unitOfWork.OrdenCompraRepository.GetOrdenParaActualizarAsync(ordenId);
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
                var lotesAnteriores = await unitOfWork.RecepcionRepository.GetLotesPorOrdenAsync(ordenId);
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
                var cantidades = recepcion.Lotes.GroupBy(item => item.ProductoId)
                    .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => (long)item.CantidadRecibida));

                foreach (var item in recepcion.Lotes)
                {
                    if (!detalles.Any(detalle => detalle.ProductoId == item.ProductoId))
                    {
                        throw new BusinessException("Un producto recibido no pertenece a la orden.", HttpStatusCode.UnprocessableEntity);
                    }
                    if (item.FechaVencimiento <= hoy)
                    {
                        throw new BusinessException("La fecha de vencimiento debe ser posterior a hoy.", HttpStatusCode.UnprocessableEntity);
                    }
                    var numeroLote = item.NumeroLote.Trim();
                    if (!claves.Add((item.ProductoId, numeroLote.ToUpperInvariant())))
                    {
                        throw new BusinessException("Se repite un lote para el mismo producto en la solicitud.", HttpStatusCode.Conflict);
                    }
                    if (await unitOfWork.RecepcionRepository.ExisteLoteAsync(item.ProductoId, numeroLote))
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
                recepcion.OrdenId = ordenId;
                recepcion.RecibidoPor = usuario.Id;
                recepcion.FechaRecepcion = ahora;
                foreach (var lote in recepcion.Lotes)
                {
                    lote.DetalleOrdenId = detalles.Single(detalle => detalle.ProductoId == lote.ProductoId).Id;
                    lote.CantidadDisponible = lote.CantidadRecibida;
                }
                orden.Estado = detalles.All(detalle =>
                    recibidos.GetValueOrDefault(detalle.Id) + cantidades.GetValueOrDefault(detalle.ProductoId)
                    == detalle.CantidadSolicitada) ? "Recibida" : "Recibida parcial";

                await unitOfWork.RecepcionRepository.Add(recepcion);
                await unitOfWork.SaveChangesAsync();
                return recepcion;
            });
        }
        catch (NumeroLoteDuplicadoException)
        {
            // Incluye colisiones concurrentes y equivalencias de la intercalacion MySQL.
            throw new BusinessException("El numero de lote ya existe para el producto.", HttpStatusCode.Conflict);
        }
    }
}
