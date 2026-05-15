using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Extensions;

public static class ValidationExtensions
{
    public static BadRequestObjectResult ValidationErrors(
        this ControllerBase controller,
        IDictionary<string, string[]> errors) =>
        controller.BadRequest(new ValidationProblemDetails(errors)
        {
            Title = "Dados invalidos.",
            Status = StatusCodes.Status400BadRequest
        });
}
