using System.Text;
using CVSPharmacy_Backend.Core.Dtos.Usuario;
using FluentValidation;

namespace CVSPharmacy_Backend.Services.Validators.Usuario;

public sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioDto>
{
    public CrearUsuarioValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Rol).NotEmpty();
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(p => p.Length >= 8 && p.Any(char.IsLetter) && p.Any(char.IsDigit))
            .WithMessage("La contraseña debe tener al menos ocho caracteres e incluir letras y números.")
            .Must(p => Encoding.UTF8.GetByteCount(p) <= 72)
            .WithMessage("La contraseña debe ocupar como máximo 72 bytes.");
    }
}

public sealed class EditarUsuarioValidator : AbstractValidator<EditarUsuarioDto>
{
    public EditarUsuarioValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Rol).NotEmpty();
    }
}
