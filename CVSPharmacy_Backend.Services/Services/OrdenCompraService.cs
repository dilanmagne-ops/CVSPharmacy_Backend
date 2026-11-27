using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;

namespace CVSPharmacy_Backend.Services.Services;

public class OrdenCompraService : IOrdenCompraService
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly GuardarOrdenCompraDtoValidator _validator;

    public OrdenCompraService(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        GuardarOrdenCompraDtoValidator validator)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IEnumerable<OrdenCompraDto>> GetAllOrdenesAsync(
        string? status,
        int? supplierId,
        DateTime? from,
        DateTime? to)
    {
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

        var ordenes = await _ordenCompraRepository.GetAll();

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

        return _mapper.Map<IEnumerable<OrdenCompraDto>>(ordenes);
    }

    public async Task<OrdenCompraDto> GetOrdenByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException(
                "El ID de la orden debe ser mayor que cero.");
        }

        var orden = await _ordenCompraRepository.GetById(id);

        if (orden is null)
        {
            throw new BusinessException(
                "Orden de compra no encontrada.",
                HttpStatusCode.NotFound);
        }

        return _mapper.Map<OrdenCompraDto>(orden);
    }
    public async Task<OrdenCompraDto> CrearOrdenAsync(
    GuardarOrdenCompraDto ordenDto,
    int usuarioId)
    {
        if (ordenDto is null)
        {
            throw new BusinessException(
                "El cuerpo de la solicitud es obligatorio.");
        }

        var validacion = await _validator.ValidateAsync(ordenDto);

        if (!validacion.IsValid)
        {
            throw new BusinessException(
                "Error de validacion.",
                HttpStatusCode.BadRequest,
                validacion.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }

        var proveedor = await _ordenCompraRepository
            .GetProveedorAsync(ordenDto.ProveedorId!.Value);

        if (proveedor is null || proveedor.Activo != true)
        {
            throw new BusinessException(
                "El proveedor no existe o esta inactivo.",
                HttpStatusCode.UnprocessableEntity);
        }

        var idsProductos = ordenDto.Items
            .Select(item => item.ProductoId!.Value)
            .Distinct()
            .ToList();

        var productos = await _ordenCompraRepository
            .GetProductosAsync(idsProductos);

        if (productos.Count != idsProductos.Count ||
            productos.Any(producto => producto.Activo != true))
        {
            throw new BusinessException(
                "Hay productos inexistentes o inactivos.",
                HttpStatusCode.UnprocessableEntity);
        }

        if (usuarioId <= 0)
        {
            throw new BusinessException(
                "Configure un usuario de pruebas valido.",
                HttpStatusCode.InternalServerError);
        }

        var usuario = await _ordenCompraRepository
            .GetUsuarioAsync(usuarioId);

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
            var numeros = await _ordenCompraRepository
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

            var orden = new OrdenesCompra
            {
                Numero = $"{prefijo}{maximo + 1:D4}",
                ProveedorId = proveedor.Id,
                CreadoPor = usuario.Id,
                Estado = "Borrador",
                FechaCreacion = ahora,
                Observaciones = ordenDto.Observaciones?.Trim(),
                DetalleOrdenCompras =
                    _mapper.Map<List<DetalleOrdenCompra>>(ordenDto.Items)
            };

            try
            {
                await _ordenCompraRepository.Add(orden);
            }
            catch (NumeroOrdenDuplicadoException)
            {
                continue;
            }

            var ordenGuardada = await _ordenCompraRepository
                .GetById(orden.Id);

            if (ordenGuardada is null)
            {
                throw new BusinessException(
                    "La orden se guardo, pero no pudo recuperarse.",
                    HttpStatusCode.InternalServerError);
            }

            return _mapper.Map<OrdenCompraDto>(ordenGuardada);
        }

        throw new BusinessException(
            "No se pudo generar un numero de orden unico. Intente nuevamente.",
            HttpStatusCode.Conflict);
    }

    public async Task<OrdenCompraDto> ActualizarOrdenAsync(
        int id, GuardarOrdenCompraDto ordenDto)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        if (ordenDto is null)
        {
            throw new BusinessException("El cuerpo de la solicitud es obligatorio.");
        }
        var validacion = await _validator.ValidateAsync(ordenDto);
        if (!validacion.IsValid)
        {
            throw new BusinessException("Error de validacion.", HttpStatusCode.BadRequest,
                validacion.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    error = error.ErrorMessage
                }).ToList());
        }

        return await _ordenCompraRepository.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await _ordenCompraRepository.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador")
            {
                throw new BusinessException("Solo se pueden editar ordenes en Borrador.", HttpStatusCode.Conflict);
            }
            if (await _ordenCompraRepository.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede editar una orden con recepciones.", HttpStatusCode.Conflict);
            }
            var proveedor = await _ordenCompraRepository.GetProveedorAsync(ordenDto.ProveedorId!.Value);
            if (proveedor is null || proveedor.Activo != true)
            {
                throw new BusinessException("El proveedor no existe o esta inactivo.", HttpStatusCode.UnprocessableEntity);
            }
            var idsProductos = ordenDto.Items.Select(item => item.ProductoId!.Value).Distinct().ToList();
            var productos = await _ordenCompraRepository.GetProductosAsync(idsProductos);
            if (productos.Count != idsProductos.Count || productos.Any(producto => producto.Activo != true))
            {
                throw new BusinessException("Hay productos inexistentes o inactivos.", HttpStatusCode.UnprocessableEntity);
            }

            orden.ProveedorId = proveedor.Id;
            orden.Observaciones = ordenDto.Observaciones?.Trim();
            _ordenCompraRepository.ReemplazarDetalles(orden,
                _mapper.Map<List<DetalleOrdenCompra>>(ordenDto.Items));
            await _ordenCompraRepository.Update(orden);

            var actualizada = await _ordenCompraRepository.GetById(id);
            if (actualizada is null)
            {
                throw new BusinessException("No se pudo recuperar la orden actualizada.", HttpStatusCode.InternalServerError);
            }
            return _mapper.Map<OrdenCompraDto>(actualizada);
        });
    }

    public async Task<OrdenCompraDto> EmitirOrdenAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        return await _ordenCompraRepository.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await _ordenCompraRepository.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador")
            {
                throw new BusinessException("Solo se puede emitir una orden en Borrador.", HttpStatusCode.Conflict);
            }
            if (await _ordenCompraRepository.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede emitir un borrador con recepciones.", HttpStatusCode.Conflict);
            }
            if (orden.DetalleOrdenCompras.Count == 0 || orden.DetalleOrdenCompras.Any(detalle =>
                detalle.CantidadSolicitada <= 0 || detalle.PrecioUnitario < 0))
            {
                throw new BusinessException("La orden debe contener detalles con cantidades y precios validos.", HttpStatusCode.UnprocessableEntity);
            }
            var proveedor = await _ordenCompraRepository.GetProveedorAsync(orden.ProveedorId);
            if (proveedor is null || proveedor.Activo != true)
            {
                throw new BusinessException("El proveedor no existe o esta inactivo.", HttpStatusCode.UnprocessableEntity);
            }
            var idsProductos = orden.DetalleOrdenCompras.Select(detalle => detalle.ProductoId).Distinct().ToList();
            var productos = await _ordenCompraRepository.GetProductosAsync(idsProductos);
            if (productos.Count != idsProductos.Count || productos.Any(producto => producto.Activo != true))
            {
                throw new BusinessException("Hay productos inexistentes o inactivos.", HttpStatusCode.UnprocessableEntity);
            }

            orden.Estado = "Emitida";
            orden.FechaEmision = DateTime.Now;
            await _ordenCompraRepository.Update(orden);
            var emitida = await _ordenCompraRepository.GetById(id);
            if (emitida is null)
            {
                throw new BusinessException("No se pudo recuperar la orden emitida.", HttpStatusCode.InternalServerError);
            }
            return _mapper.Map<OrdenCompraDto>(emitida);
        });
    }

    public async Task<OrdenCompraDto> CancelarOrdenAsync(int id)
    {
        if (id <= 0)
        {
            throw new BusinessException("El ID de la orden debe ser mayor que cero.");
        }
        return await _ordenCompraRepository.EjecutarEnTransaccionAsync(async () =>
        {
            var orden = await _ordenCompraRepository.GetOrdenParaActualizarAsync(id);
            if (orden is null)
            {
                throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
            }
            if (orden.Estado != "Borrador" && orden.Estado != "Emitida")
            {
                throw new BusinessException("Solo se pueden cancelar ordenes en Borrador o Emitida.", HttpStatusCode.Conflict);
            }
            if (await _ordenCompraRepository.TieneRecepcionesAsync(id))
            {
                throw new BusinessException("No se puede cancelar una orden con recepciones.", HttpStatusCode.Conflict);
            }

            orden.Estado = "Cancelada";
            await _ordenCompraRepository.Update(orden);
            var cancelada = await _ordenCompraRepository.GetById(id);
            if (cancelada is null)
            {
                throw new BusinessException("No se pudo recuperar la orden cancelada.", HttpStatusCode.InternalServerError);
            }
            return _mapper.Map<OrdenCompraDto>(cancelada);
        });
    }
}
