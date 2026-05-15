using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Extensions;

public static class ValidationExtensions
{
    public static BadRequestObjectResult ValidationErrors(
        this ControllerBase controller,
        IReadOnlyDictionary<string, string[]> errors) =>
        controller.BadRequest(new ValidationProblemDetails(errors.ToDictionary(item => item.Key, item => item.Value))
        {
            Title = "Dados inválidos.",
            Status = StatusCodes.Status400BadRequest
        });
}
