using System.Net;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Core.Entities;

namespace CVSPharmacy_Backend.Services.Services;

public class OrdenCompraService(IUnitOfWork unitOfWork) : IOrdenCompraService
{
    private IOrdenCompraRepository Ordenes => unitOfWork.OrdenCompraRepository;

    public async Task<IEnumerable<OrdenesCompra>> GetAllOrdenesAsync(OrdenCompraQueryFilters filters)
    {
        var status = filters.Status;
        var supplierId = filters.SupplierId;
        var from = filters.From;
        var to = filters.To;
        if (supplierId.HasValue && supplierId.Value <= 0)
        {
            throw new BusinessException(
                "El ID del proveedor debe ser mayor que cero.");
        }

        if (from.HasValue && to.HasValue &&
            from.Value.Date > to.Value.Date)
        {
            throw new BusinessException(
                "La fecha inicial no puede ser posterior a la fecha final.");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim();

            if (status != "Borrador" &&
                status != "Emitida" &&
                status != "Recibida parcial" &&
                status != "Recibida" &&
                status != "Cancelada")
            {
                throw new BusinessException(
                    "El estado de la orden no es valido.");
            }
        }

        var ordenes = await Ordenes.GetAll();

        if (!string.IsNullOrWhiteSpace(status))
        {
            ordenes = ordenes.Where(orden => orden.Estado == status);
        }

        if (supplierId.HasValue)
        {
            ordenes = ordenes.Where(orden => orden.ProveedorId == supplierId.Value);
        }

        if (from.HasValue)
        {
            ordenes = ordenes.Where(orden => orden.FechaCreacion.Date >= from.Value.Date);
        }

        if (to.HasValue)
        {
            ordenes = ordenes.Where(orden => orden.FechaCreacion.Date <= to.Value.Date);
        }

        return ordenes;
    }

    public async Task<OrdenesCompra> GetOrdenByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException(
                "El ID de la orden debe ser mayor que cero.");
        }

        var orden = await Ordenes.GetById(id);

        if (orden is null)
        {
            throw new BusinessException(
                "Orden de compra no encontrada.",
                HttpStatusCode.NotFound);
        }

        return orden;
    }
    public async Task<OrdenesCompra> CrearOrdenAsync(OrdenesCompra solicitud, int usuarioId)
    {
        await ValidarProveedorYProductosAsync(solicitud);
        if (usuarioId <= 0)
        {
            throw new BusinessException(
                "Configure un usuario de pruebas valido.",
                HttpStatusCode.InternalServerError);
        }

        var usuario = await unitOfWork.UsuarioRepository.GetById(usuarioId);

        if (usuario is null || usuario.Activo != true)
        {
            throw new BusinessException(
                "El usuario de pruebas no existe o esta inactivo.",
                HttpStatusCode.Forbidden);
        }

        var ahora = DateTime.Now;
        var prefijo = $"OC-{ahora.Year}-";

        for (var intento = 0; intento < 3; intento++)
        {
            var numeros = await Ordenes
                .GetNumerosOrdenAsync(prefijo);

            long maximo = 0;

            foreach (var numero in numeros)
            {
                var parteNumerica = numero.Substring(prefijo.Length);

                if (long.TryParse(parteNumerica, out var correlativo) &&
                    correlativo > maximo)
                {
                    maximo = correlativo;
                }
            }

            // La columna numero admite 20 caracteres:
            // 8 del prefijo OC-AAAA- y hasta 12 del correlativo.
            if (maximo >= 999999999999L)
            {
                throw new BusinessException(
                    "Se agoto la numeracion anual de ordenes.",
                    HttpStatusCode.Conflict);
            }

            solicitud.Numero = $"{prefijo}{maximo + 1:D4}";
            solicitud.CreadoPor = usuario.Id;
            solicitud.Estado = "Borrador";
            solicitud.FechaCreacion = ahora;

            try
            {
                await Ordenes.Add(solicitud);
                await unitOfWork.SaveChangesAsync();
            }
            catch (NumeroOrdenDuplicadoException)
            {
                continue;
            }

            var ordenGuardada = await Ordenes
                .GetById(solicitud.Id);

            if (ordenGuardada is null)
            {
                throw new BusinessException(
                    "La orden se guardo, pero no pudo recuperarse.",
                    HttpStatusCode.InternalServerError);
            }

            return ordenGuardada;
        }

        throw new BusinessException(
            "No se pudo generar un numero de orden unico. Intente nuevamente.",
            HttpStatusCode.Conflict);
    }

    public async Task<OrdenesCompra> ActualizarOrdenAsync(
        int id, OrdenesCompra solicitud)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        return await Ordenes.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await Ordenes.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador")
            {
                throw new BusinessException("Solo se pueden editar ordenes en Borrador.", HttpStatusCode.Conflict);
            }
            if (await Ordenes.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede editar una orden con recepciones.", HttpStatusCode.Conflict);
            }
            await ValidarProveedorYProductosAsync(solicitud);
            orden.ProveedorId = solicitud.ProveedorId;
            orden.Observaciones = solicitud.Observaciones;
            Ordenes.ReemplazarDetalles(orden, solicitud.DetalleOrdenCompras);
            await unitOfWork.SaveChangesAsync();

            var actualizada = await Ordenes.GetById(id);
            if (actualizada is null)
            {
                throw new BusinessException("No se pudo recuperar la orden actualizada.", HttpStatusCode.InternalServerError);
            }
            return actualizada;
        });
    }

    public async Task<OrdenesCompra> EmitirOrdenAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        return await Ordenes.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await Ordenes.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador")
            {
                throw new BusinessException("Solo se puede emitir una orden en Borrador.", HttpStatusCode.Conflict);
            }
            if (await Ordenes.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede emitir un borrador con recepciones.", HttpStatusCode.Conflict);
            }
            if (orden.DetalleOrdenCompras.Count == 0 || orden.DetalleOrdenCompras.Any(detalle =>
                detalle.CantidadSolicitada <= 0 || detalle.PrecioUnitario < 0))
            {
                throw new BusinessException("La orden debe contener detalles con cantidades y precios validos.", HttpStatusCode.UnprocessableEntity);
            }
            await ValidarProveedorYProductosAsync(orden);

            orden.Estado = "Emitida";
            orden.FechaEmision = DateTime.Now;
            await unitOfWork.SaveChangesAsync();
            var emitida = await Ordenes.GetById(id);
            if (emitida is null)
            {
                throw new BusinessException("No se pudo recuperar la orden emitida.", HttpStatusCode.InternalServerError);
            }
            return emitida;
        });
    }

    public async Task<OrdenesCompra> CancelarOrdenAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        return await Ordenes.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await Ordenes.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador" && orden.Estado != "Emitida")
            {
                throw new BusinessException("Solo se pueden cancelar ordenes en Borrador o Emitida.", HttpStatusCode.Conflict);
            }
            if (await Ordenes.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede cancelar una orden con recepciones.", HttpStatusCode.Conflict);
            }

            orden.Estado = "Cancelada";
            await unitOfWork.SaveChangesAsync();
            var cancelada = await Ordenes.GetById(id);
            if (cancelada is null)
            {
                throw new BusinessException("No se pudo recuperar la orden cancelada.", HttpStatusCode.InternalServerError);
            }
            return cancelada;
        });
    }

    private async Task ValidarProveedorYProductosAsync(OrdenesCompra orden)
    {
        var proveedor = await unitOfWork.ProveedorRepository.GetById(orden.ProveedorId);
        if (proveedor?.Activo != true)
            throw new BusinessException("El proveedor no existe o esta inactivo.", HttpStatusCode.UnprocessableEntity);

        var ids = orden.DetalleOrdenCompras.Select(detalle => detalle.ProductoId).Distinct().ToList();
        var productos = await unitOfWork.ProductoRepository.GetProductosAsync(ids);
        if (productos.Count != ids.Count || productos.Any(producto => producto.Activo != true))
            throw new BusinessException("Hay productos inexistentes o inactivos.", HttpStatusCode.UnprocessableEntity);
    }
}
