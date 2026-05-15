using FrangoFrito.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Extensions;

public static class ApplicationResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ApplicationResult<T> result, ControllerBase controller) =>
        result.Status switch
        {
            ApplicationResultStatus.Success => controller.Ok(result.Value),
            ApplicationResultStatus.NotFound => controller.NotFound(),
            ApplicationResultStatus.Conflict => controller.Conflict(CreateProblemDetails(result, StatusCodes.Status409Conflict)),
            ApplicationResultStatus.ValidationError => controller.ValidationErrors(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            ApplicationResultStatus.Unauthorized => controller.Problem(title: result.Error?.Title, detail: result.Error?.Detail, statusCode: StatusCodes.Status401Unauthorized),
            ApplicationResultStatus.Forbidden => controller.Forbid(),
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };

    public static IActionResult ToActionResult(this ApplicationResult result, ControllerBase controller) =>
        result.Status switch
        {
            ApplicationResultStatus.Success => controller.NoContent(),
            ApplicationResultStatus.NotFound => controller.NotFound(),
            ApplicationResultStatus.Conflict => controller.Conflict(CreateProblemDetails(result, StatusCodes.Status409Conflict)),
            ApplicationResultStatus.ValidationError => controller.ValidationErrors(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            ApplicationResultStatus.Unauthorized => controller.Problem(title: result.Error?.Title, detail: result.Error?.Detail, statusCode: StatusCodes.Status401Unauthorized),
            ApplicationResultStatus.Forbidden => controller.Forbid(),
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };

    private static ProblemDetails CreateProblemDetails(ApplicationResult result, int statusCode) =>
        new()
        {
            Title = result.Error?.Title,
            Detail = result.Error?.Detail,
            Status = statusCode
        };
}
