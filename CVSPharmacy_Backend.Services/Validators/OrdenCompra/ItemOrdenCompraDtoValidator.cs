using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.OrdenCompra;

public class ItemOrdenCompraDtoValidator : AbstractValidator<ItemOrdenCompraDto>
{
    public ItemOrdenCompraDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El producto es obligatorio.")
            .GreaterThan(0).WithMessage("El ID del producto debe ser mayor que cero.");

        RuleFor(x => x.CantidadSolicitada)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La cantidad solicitada es obligatoria.")
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que cero.");

        RuleFor(x => x.PrecioUnitario)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El precio unitario es obligatorio.")
            .GreaterThanOrEqualTo(0)
                .WithMessage("El precio unitario no puede ser negativo.")
            .PrecisionScale(10, 2, true)
                .WithMessage("El precio admite hasta 8 digitos enteros y 2 decimales.");
    }
}
