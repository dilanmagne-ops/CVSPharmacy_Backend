using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.OrdenCompra;

// Validación de formato (responde 400). Las reglas que consultan la base
// (proveedor activo, productos activos) se validan en el Service (422).
public class GuardarOrdenCompraDtoValidator : AbstractValidator<GuardarOrdenCompraDto>
{
    public GuardarOrdenCompraDtoValidator()
    {
        RuleFor(x => x.ProveedorId)
            .NotNull().WithMessage("El proveedor es obligatorio.")
            .GreaterThan(0).WithMessage("El proveedor no es válido.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(255).WithMessage("Las observaciones no pueden superar 255 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe tener al menos un ítem.");

        RuleForEach(x => x.Items)
            .SetValidator(new ItemOrdenCompraDtoValidator());
    }
}

public class ItemOrdenCompraDtoValidator : AbstractValidator<ItemOrdenCompraDto>
{
    public ItemOrdenCompraDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotNull().WithMessage("El producto es obligatorio.")
            .GreaterThan(0).WithMessage("El producto no es válido.");

        RuleFor(x => x.CantidadSolicitada)
            .NotNull().WithMessage("La cantidad es obligatoria.")
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.PrecioUnitario)
            .NotNull().WithMessage("El precio unitario es obligatorio.")
            .GreaterThanOrEqualTo(0m).WithMessage("El precio unitario no puede ser negativo.");
    }
}
