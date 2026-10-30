using System.Net;
using AutoMapper;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;
namespace CVSPharmacy_Backend.Services.Services;

public sealed class OrdenCompraService(IBaseRepository<OrdenesCompra> ordenes,
    IBaseRepository<DetalleOrdenCompra> detalles, IBaseRepository<Proveedor> proveedores,
    IBaseRepository<Producto> productos, IBaseRepository<Recepcion> recepciones,
    IUnidadTrabajo unidad, IUsuarioActual usuario, IMapper mapper, GuardarOrdenCompraDtoValidator validator)
    : IOrdenCompraService
{
    public async Task<IEnumerable<OrdenCompraDto>> GetAllOrdenesAsync(string? estado, int? proveedorId, DateTime? desde, DateTime? hasta)
    {
        await usuario.ObtenerAsync();
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new BusinessException("La fecha desde no puede ser posterior a hasta.");
        // Se filtra antes de materializar. El detalle incluye el producto usado por el perfil.
        var all = await ordenes.GetAllAsync(o =>
            (estado == null || o.Estado == estado) && (!proveedorId.HasValue || o.ProveedorId == proveedorId)
            && (!desde.HasValue || o.FechaCreacion >= desde.Value.Date)
            && (!hasta.HasValue || o.FechaCreacion < hasta.Value.Date.AddDays(1)), o => o.Proveedor);
        var result = new List<OrdenCompraDto>();
        foreach (var orden in all.OrderByDescending(o => o.FechaCreacion))
            result.Add(await MapearAsync(orden));
        return result;
    }
    public async Task<OrdenCompraDto> GetOrdenByIdAsync(int id)
    {
        await usuario.ObtenerAsync();
        return await LeerAsync(id);
    }
    private async Task<OrdenCompraDto> LeerAsync(int id)
    {
        var orden = await ordenes.GetByIdAsync(id, o => o.Proveedor)
            ?? throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
        return await MapearAsync(orden);
    }
    private async Task<OrdenCompraDto> MapearAsync(OrdenesCompra orden)
    {
        orden.DetalleOrdenCompras = (await detalles.GetAllAsync(d => d.OrdenId == orden.Id, d => d.Producto)).ToList();
        return mapper.Map<OrdenCompraDto>(orden);
    }
    private async Task ValidarAsync(GuardarOrdenCompraDto dto)
    {
        await ValidacionEntrada.ValidarAsync(validator, dto);
        var proveedor = await proveedores.GetByIdAsync(dto.ProveedorId!.Value);
        if (proveedor?.Activo != true) throw new BusinessException("El proveedor no existe o está inactivo.", HttpStatusCode.UnprocessableEntity);
        var ids = dto.Items.Select(i => i.ProductoId!.Value).ToList();
        var validos = await productos.GetAllAsync(p => ids.Contains(p.Id) && p.Activo == true);
        if (validos.Count != ids.Count) throw new BusinessException("Hay productos inexistentes o inactivos.", HttpStatusCode.UnprocessableEntity);
    }
    public async Task<OrdenCompraDto> CrearOrdenAsync(GuardarOrdenCompraDto dto)
    {
        var actor = await usuario.ObtenerAsync();
        await ValidarAsync(dto);
        // Correlativo numérico por año; el índice único y la transacción permiten reintentar una colisión.
        for (var intento = 0; intento < 3; intento++)
        {
            try
            {
                return await unidad.EjecutarAsync(async () =>
                {
                    var orden = new OrdenesCompra
                    {
                        Numero = await GenerarNumeroAsync(),
                        ProveedorId = dto.ProveedorId!.Value, CreadoPor = actor.Id,
                        Estado = EstadosOrden.Borrador, FechaCreacion = DateTime.Now,
                        Observaciones = dto.Observaciones?.Trim(),
                        DetalleOrdenCompras = mapper.Map<List<DetalleOrdenCompra>>(dto.Items)
                    };
                    await ordenes.AddAsync(orden); await ordenes.SaveAsync();
                    return await LeerAsync(orden.Id);
                });
            }
            catch (DuplicateKeyException)
            {
                if (intento == 2) throw new BusinessException("No se pudo generar un número de orden único. Reintente.", HttpStatusCode.Conflict);
            }
        }
        throw new BusinessException("No se pudo generar el número de orden.", HttpStatusCode.Conflict);
    }
    private async Task<string> GenerarNumeroAsync()
    {
        var prefijo = $"OC-{DateTime.Today.Year}-";
        var existentes = await ordenes.GetAllAsync(o => o.Numero.StartsWith(prefijo));
        var maximo = existentes.Select(o => long.TryParse(o.Numero[prefijo.Length..], out var n) ? n : 0L)
            .DefaultIfEmpty(0).Max();
        if (maximo >= 999999999999L) throw new BusinessException("Se agotó la numeración anual.", HttpStatusCode.Conflict);
        return $"{prefijo}{maximo + 1:D4}";
    }
    public async Task<OrdenCompraDto> ActualizarOrdenAsync(int id, GuardarOrdenCompraDto dto)
    {
        await usuario.ObtenerAsync();
        await ValidacionEntrada.ValidarAsync(validator, dto);
        return await unidad.EjecutarAsync(async () =>
        {
            var orden = await BloquearAsync(id);
            if (orden.Estado != EstadosOrden.Borrador) throw new BusinessException("Solo se pueden editar órdenes en Borrador.", HttpStatusCode.Conflict);
            await ValidarAsync(dto);
            var anteriores = await detalles.GetAllAsync(d => d.OrdenId == id);
            detalles.RemoveRange(anteriores);
            orden.ProveedorId = dto.ProveedorId!.Value; orden.Observaciones = dto.Observaciones?.Trim();
            orden.DetalleOrdenCompras = mapper.Map<List<DetalleOrdenCompra>>(dto.Items);
            await ordenes.SaveAsync();
            return await LeerAsync(id);
        });
    }
    public async Task<OrdenCompraDto> EmitirOrdenAsync(int id)
    {
        await usuario.ObtenerAsync(true);
        return await unidad.EjecutarAsync(async () =>
        {
            var orden = await BloquearAsync(id);
            if (orden.Estado != EstadosOrden.Borrador) throw new BusinessException("Solo se puede emitir una orden en Borrador.", HttpStatusCode.Conflict);
            orden.Estado = EstadosOrden.Emitida; orden.FechaEmision = DateTime.Now;
            await ordenes.SaveAsync(); return await LeerAsync(id);
        });
    }
    public async Task<OrdenCompraDto> CancelarOrdenAsync(int id)
    {
        await usuario.ObtenerAsync(true);
        return await unidad.EjecutarAsync(async () =>
        {
            var orden = await BloquearAsync(id);
            if (orden.Estado != EstadosOrden.Borrador && orden.Estado != EstadosOrden.Emitida)
                throw new BusinessException("La orden no admite cancelación en su estado actual.", HttpStatusCode.Conflict);
            if (await recepciones.AnyAsync(r => r.OrdenId == id))
                throw new BusinessException("La orden tiene recepciones y no puede cancelarse.", HttpStatusCode.Conflict);
            orden.Estado = EstadosOrden.Cancelada; await ordenes.SaveAsync(); return await LeerAsync(id);
        });
    }
    private async Task<OrdenesCompra> BloquearAsync(int id) => await unidad.BloquearOrdenAsync(id)
        ?? throw new BusinessException("Orden de compra no encontrada.", HttpStatusCode.NotFound);
}
