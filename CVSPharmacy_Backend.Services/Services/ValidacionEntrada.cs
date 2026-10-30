using FluentValidation;
namespace CVSPharmacy_Backend.Services.Services;
internal static class ValidacionEntrada
{
    public static async Task ValidarAsync<T>(IValidator<T> validator, T dto) where T : class
    {
        if (dto is null) throw new CVSPharmacy_Backend.Core.Exceptions.BusinessException("El cuerpo de la solicitud es obligatorio.");
        var resultado = await validator.ValidateAsync(dto);
        if (!resultado.IsValid) throw new ValidationException(resultado.Errors);
    }
}
