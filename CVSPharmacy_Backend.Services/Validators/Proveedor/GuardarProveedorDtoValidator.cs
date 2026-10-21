using CVSPharmacy_Backend.Core.Dtos;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Proveedor;

public sealed class GuardarProveedorDtoValidator : AbstractValidator<GuardarProveedorDto>
{
    public GuardarProveedorDtoValidator()
    {
        RuleFor(proveedor => proveedor.Nit)
            .Must(nit => !string.IsNullOrWhiteSpace(nit))
            .WithMessage("El NIT es obligatorio.")
            .Must(nit => nit is null || nit.Trim().Length <= 20)
            .WithMessage("El NIT no puede superar los 20 caracteres.");

        RuleFor(proveedor => proveedor.RazonSocial)
            .Must(razonSocial => !string.IsNullOrWhiteSpace(razonSocial))
            .WithMessage("La razón social es obligatoria.")
            .Must(razonSocial => razonSocial is null || razonSocial.Trim().Length <= 150)
            .WithMessage("La razón social no puede superar los 150 caracteres.");

        RuleFor(proveedor => proveedor.Contacto)
            .Must(contacto => contacto is null || contacto.Trim().Length <= 100)
            .WithMessage("El contacto no puede superar los 100 caracteres.");

        RuleFor(proveedor => proveedor.Telefono)
            .Must(telefono => telefono is null || telefono.Trim().Length <= 30)
            .WithMessage("El teléfono no puede superar los 30 caracteres.");

        RuleFor(proveedor => proveedor.Email)
            .Must(email => email is null || email.Trim().Length <= 255)
            .WithMessage("El correo no puede superar los 255 caracteres.");

        RuleFor(proveedor => proveedor.Email)
            .EmailAddress()
            .When(proveedor => !string.IsNullOrWhiteSpace(proveedor.Email))
            .WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(proveedor => proveedor.Direccion)
            .Must(direccion => direccion is null || direccion.Trim().Length <= 200)
            .WithMessage("La dirección no puede superar los 200 caracteres.");
    }
}
