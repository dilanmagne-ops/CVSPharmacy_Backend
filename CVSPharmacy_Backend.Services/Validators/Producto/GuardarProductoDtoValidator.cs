using FluentValidation;
using CVSPharmacy_Backend.Core.Dtos.Producto;

namespace CVSPharmacy_Backend.Services.Validators.Producto
{
    public class GuardarProductoDtoValidator : AbstractValidator<GuardarProductoDto>
    {
        public GuardarProductoDtoValidator()
        {
            //Estructura SQL; las validaciones de precio y stock mínimo corresponden a la RN-06
            // Validar codigo
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("El codigo es obligatorio.")
                .MaximumLength(30).WithMessage("El codigo no puede superar los 30 caracteres.");

            // Validar nombre
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

            // Validar principio activo (opcional)
            RuleFor(x => x.PrincipioActivo)
                .MaximumLength(150).WithMessage("El principio activo no puede superar los 150 caracteres.");

            // Validar presentacion
            RuleFor(x => x.Presentacion)
                .NotEmpty().WithMessage("La presentacion es obligatoria.")
                .MaximumLength(100).WithMessage("La presentacion no puede superar los 100 caracteres.");

            // Validar unidad de medida
            RuleFor(x => x.UnidadMedida)
                .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
                .MaximumLength(20).WithMessage("La unidad de medida no puede superar los 20 caracteres.");

            // Validar precio de venta
            RuleFor(x => x.PrecioVenta)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("El precio de venta es obligatorio.")
                .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.")
                .PrecisionScale(10, 2, true).WithMessage("El precio debe tener maximo 8 digitos enteros y 2 decimales.");

            // Validar stock minimo
            RuleFor(x => x.StockMinimo)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("El stock minimo es obligatorio.")
                .GreaterThanOrEqualTo(0).WithMessage("El stock minimo no puede ser negativo.");
        }
    }
}
