using CVSPharmacy_Backend.Core.Dtos.Producto;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Producto;

public sealed class CambiarEstadoProductoDtoValidator : AbstractValidator<CambiarEstadoProductoDto>
{
    public CambiarEstadoProductoDtoValidator()
    {
        RuleFor(estado => estado.Activo)
            .NotNull().WithMessage("El estado activo es obligatorio.");
    }
}