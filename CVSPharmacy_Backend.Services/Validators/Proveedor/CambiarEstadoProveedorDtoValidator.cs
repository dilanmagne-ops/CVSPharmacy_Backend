using CVSPharmacy_Backend.Core.Dtos.Proveedor;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Proveedor;

public sealed class CambiarEstadoProveedorDtoValidator : AbstractValidator<CambiarEstadoProveedorDto>
{
    public CambiarEstadoProveedorDtoValidator()
    {
        RuleFor(estado => estado.Activo)
            .NotNull().WithMessage("El estado activo es obligatorio.");
    }
}