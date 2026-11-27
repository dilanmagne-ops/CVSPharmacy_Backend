using System.Net;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Core.QueryFilters;
using CVSPharmacy_Backend.Services.Interfaces;

namespace CVSPharmacy_Backend.Services.Services;

public sealed class ProveedorService(IUnitOfWork unitOfWork) : IProveedorService
{
    private IProveedorRepository Proveedores => unitOfWork.ProveedorRepository;

    public async Task<IReadOnlyList<Proveedor>> GetAllAsync(ProveedorQueryFilters filters)
    {
        var proveedores = await Proveedores.GetAll();
        if (!string.IsNullOrWhiteSpace(filters.Q))
        {
            var busqueda = filters.Q.Trim();
            proveedores = proveedores.Where(p =>
                p.Nit.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                p.RazonSocial.Contains(busqueda, StringComparison.OrdinalIgnoreCase));
        }
        if (filters.Active.HasValue)
            proveedores = proveedores.Where(p => p.Activo == filters.Active.Value);

        return proveedores.ToList();
    }

    public async Task<Proveedor> GetByIdAsync(int id)
    {
        return await Proveedores.GetById(id) ?? throw NoEncontrado();
    }

    public async Task<Proveedor> CreateAsync(Proveedor proveedor)
    {
        await ValidarNitAsync(proveedor.Nit);
        proveedor.Activo = true;
        await Proveedores.Add(proveedor);
        await GuardarAsync();
        return proveedor;
    }

    public async Task<Proveedor> UpdateAsync(int id, Proveedor cambios)
    {
        var proveedor = await Proveedores.GetForUpdateAsync(id) ?? throw NoEncontrado();
        await ValidarNitAsync(cambios.Nit, id);

        proveedor.Nit = cambios.Nit;
        proveedor.RazonSocial = cambios.RazonSocial;
        proveedor.Contacto = cambios.Contacto;
        proveedor.Telefono = cambios.Telefono;
        proveedor.Email = cambios.Email;
        proveedor.Direccion = cambios.Direccion;

        await GuardarAsync();
        return proveedor;
    }

    public async Task<Proveedor> ChangeStatusAsync(int id, bool activo)
    {
        var proveedor = await Proveedores.GetForUpdateAsync(id) ?? throw NoEncontrado();
        proveedor.Activo = activo;
        await unitOfWork.SaveChangesAsync();
        return proveedor;
    }

    private async Task ValidarNitAsync(string nit, int? excludeId = null)
    {
        if (await Proveedores.NitExistsAsync(nit, excludeId))
            throw NitDuplicado();
    }

    private async Task GuardarAsync()
    {
        try
        {
            await unitOfWork.SaveChangesAsync();
        }
        catch (NitProveedorDuplicadoException)
        {
            throw NitDuplicado();
        }
    }

    private static BusinessException NitDuplicado() =>
        new("El NIT ya pertenece a un proveedor registrado.", HttpStatusCode.Conflict);

    private static BusinessException NoEncontrado() =>
        new("Proveedor no encontrado.", HttpStatusCode.NotFound);
}
