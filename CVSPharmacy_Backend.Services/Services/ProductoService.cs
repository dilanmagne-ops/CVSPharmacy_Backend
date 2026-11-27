using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public class ProductoService(IUnitOfWork unitOfWork) : IProductoService
{
    private IProductoRepository Productos => unitOfWork.ProductoRepository;

    public async Task<IEnumerable<Producto>> GetAllProductosAsync(ProductoQueryFilters filters)
    {
        var productos = await Productos.GetAll();
        if (!string.IsNullOrWhiteSpace(filters.Q))
        {
            productos = productos.Where(p => p.Codigo.Contains(filters.Q, StringComparison.OrdinalIgnoreCase)
                || p.Nombre.Contains(filters.Q, StringComparison.OrdinalIgnoreCase));
        }
        if (filters.Active.HasValue)
        {
            productos = productos.Where(p => p.Activo == filters.Active.Value);
        }
        return productos;
    }

    public async Task<Producto> GetProductoByIdAsync(int id)
    {
        return await Productos.GetById(id) ?? throw ProductoNoEncontrado();
    }

    public Task<IEnumerable<Producto>> GetProductosStockCriticoAsync()
    {
        return Productos.GetProductosStockCriticoAsync();
    }

    public async Task<Producto> CrearProductoAsync(Producto producto)
    {
        const string conflicto = "El codigo del producto ya existe.";
        if (await Productos.GetProductoByCodigoAsync(producto.Codigo) != null)
            throw new BusinessException(conflicto, HttpStatusCode.Conflict);

        producto.Activo = true;
        await Productos.Add(producto);
        await GuardarProductoAsync(conflicto);
        return await GetProductoByIdAsync(producto.Id);
    }

    public async Task<Producto> ActualizarProductoAsync(int id, Producto cambios)
    {
        var producto = await Productos.GetProductoParaActualizarAsync(id) ?? throw ProductoNoEncontrado();
        const string conflicto = "El codigo pertenece a otro producto.";
        var existente = await Productos.GetProductoByCodigoAsync(cambios.Codigo);
        if (existente != null && existente.Id != id)
            throw new BusinessException(conflicto, HttpStatusCode.Conflict);

        producto.Codigo = cambios.Codigo;
        producto.Nombre = cambios.Nombre;
        producto.PrincipioActivo = cambios.PrincipioActivo;
        producto.Presentacion = cambios.Presentacion;
        producto.UnidadMedida = cambios.UnidadMedida;
        producto.PrecioVenta = cambios.PrecioVenta;
        producto.StockMinimo = cambios.StockMinimo;

        await GuardarProductoAsync(conflicto);
        return await GetProductoByIdAsync(id);
    }

    public async Task<bool> CambiarEstadoProductoAsync(int id, bool activo)
    {
        var producto = await Productos.GetProductoParaActualizarAsync(id) ?? throw ProductoNoEncontrado();
        producto.Activo = activo;
        await unitOfWork.SaveChangesAsync();
        return activo;
    }

    private async Task GuardarProductoAsync(string mensajeConflicto)
    {
        try
        {
            await unitOfWork.SaveChangesAsync();
        }
        catch (CodigoProductoDuplicadoException)
        {
            throw new BusinessException(mensajeConflicto, HttpStatusCode.Conflict);
        }
    }

    private static BusinessException ProductoNoEncontrado() =>
        new("Producto no encontrado.", HttpStatusCode.NotFound);
}
