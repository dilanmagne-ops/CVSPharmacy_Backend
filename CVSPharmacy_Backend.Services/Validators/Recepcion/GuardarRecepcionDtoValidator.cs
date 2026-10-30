using FluentValidation;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
namespace CVSPharmacy_Backend.Services.Validators.Recepcion;
public sealed class GuardarRecepcionDtoValidator : AbstractValidator<GuardarRecepcionDto>
{
    public GuardarRecepcionDtoValidator()
    {
        RuleFor(r => r.NroRemision).MaximumLength(40);
        RuleFor(r => r.Observaciones).MaximumLength(255);
        RuleFor(r => r.Items).NotEmpty().WithMessage("Debe indicar al menos un lote recibido.");
        RuleForEach(r => r.Items).NotNull().WithMessage("El lote no puede ser nulo.");
        RuleForEach(r => r.Items).SetValidator(new LoteRecibidoDtoValidator());
    }
}
public sealed class LoteRecibidoDtoValidator : AbstractValidator<LoteRecibidoDto>
{
    public LoteRecibidoDtoValidator()
    {
        RuleFor(l => l.ProductoId).NotNull().GreaterThan(0);
        RuleFor(l => l.Cantidad).NotNull().GreaterThan(0);
        RuleFor(l => l.NumeroLote).Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("El número de lote es obligatorio.")
            .Must(n => n is null || n.Trim().Length <= 40).WithMessage("El lote admite hasta 40 caracteres.");
        RuleFor(l => l.FechaVencimiento).NotNull().WithMessage("La fecha de vencimiento es obligatoria.");
    }
}
