using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Recepcion;

public class GuardarRecepcionDtoValidator : AbstractValidator<GuardarRecepcionDto>
{
    public GuardarRecepcionDtoValidator()
    {
        RuleFor(x => x.NroRemision)
            .MaximumLength(40).WithMessage("La remision no puede superar los 40 caracteres.");
        RuleFor(x => x.Observaciones)
            .MaximumLength(255).WithMessage("Las observaciones no pueden superar los 255 caracteres.");
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe indicar al menos un lote recibido.");
        RuleForEach(x => x.Items)
            .NotNull().WithMessage("El lote recibido no puede ser nulo.");
        RuleForEach(x => x.Items)
            .SetValidator(new LoteRecibidoDtoValidator());
    }
}
