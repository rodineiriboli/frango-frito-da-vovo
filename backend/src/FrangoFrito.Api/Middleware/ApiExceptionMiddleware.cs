using FrangoFrito.Application.Common;
using FrangoFrito.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
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
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Regra de negócio violada.", exception.Message);
        }
        catch (ConcurrencyConflictException exception)
        {
            _logger.LogWarning(exception, "Conflito de concorrência ao salvar dados.");
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflito de concorrência.", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro inesperado na API.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Erro interno.", "Não foi possível concluir a operação.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
