using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Recepcion;

public class LoteRecibidoDtoValidator : AbstractValidator<LoteRecibidoDto>
{
    public LoteRecibidoDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("El producto es obligatorio.")
            .GreaterThan(0).WithMessage("El ID del producto debe ser mayor que cero.");
        RuleFor(x => x.Cantidad)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La cantidad es obligatoria.")
            .GreaterThan(0).WithMessage("La cantidad recibida debe ser mayor que cero.");
        RuleFor(x => x.NumeroLote)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El numero de lote es obligatorio.")
            .Must(numero => numero.Trim().Length <= 40)
                .WithMessage("El numero de lote no puede superar los 40 caracteres.");
        RuleFor(x => x.FechaVencimiento)
            .NotNull().WithMessage("La fecha de vencimiento es obligatoria.");
    }
}
