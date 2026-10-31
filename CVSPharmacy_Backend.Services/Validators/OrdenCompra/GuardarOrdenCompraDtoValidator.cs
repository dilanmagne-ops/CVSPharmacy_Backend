using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.OrdenCompra;

public class GuardarOrdenCompraDtoValidator : AbstractValidator<GuardarOrdenCompraDto>
{
    public GuardarOrdenCompraDtoValidator()
    {
        RuleFor(x => x.ProveedorId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El proveedor es obligatorio.")
            .GreaterThan(0).WithMessage("El ID del proveedor debe ser mayor que cero.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(255)
            .WithMessage("Las observaciones no pueden superar los 255 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("La orden debe tener al menos un item.");

        RuleFor(x => x.Items)
            .Must(items => items is null ||
                items.Where(item => item is not null)
                    .Select(item => item.ProductoId)
                    .Distinct().Count() ==
                items.Count(item => item is not null))
            .WithMessage("No se puede repetir un producto en la orden.");

        RuleForEach(x => x.Items)
            .NotNull()
            .WithMessage("El item no puede ser nulo.");

        RuleForEach(x => x.Items)
            .SetValidator(new ItemOrdenCompraDtoValidator());
    }
}
