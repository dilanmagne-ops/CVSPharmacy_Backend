using System.Net;
using System.Text.Json;
using FluentValidation;
using CVSPharmacy_Backend.Core.CustomEntities;
using CVSPharmacy_Backend.Core.Exceptions;
namespace CVSPharmacy_Backend.Api.Filters
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
            catch (Exception ex)
            {
                if (context.Response.HasStarted) throw;
                _logger.LogError(ex, "Excepción capturada por el middleware global.");
                context.Response.Clear();
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            string message = "Ocurrio un error interno.";
            object? errors = null;

            switch (exception)
            {
                case BusinessException businessException:
                    code = businessException.StatusCode;
                    message = businessException.Message;
                    errors = businessException.Details;
                    break;

                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    message = "Error de validacion.";

                    errors = validationException.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    });
                    break;
            }

            var response = new ErrorResponse
            {
                Status = (int)code,
                Title = code.ToString(),
                Message = message,
                Errors = errors,
                TraceId = context.TraceIdentifier
            };

            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
