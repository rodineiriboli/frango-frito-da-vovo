using FrangoFrito.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Regra de negocio violada.", exception.Message);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogWarning(exception, "Conflito de concorrencia ao salvar dados.");
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflito de concorrencia.", "O registro foi alterado por outro processo. Recarregue e tente novamente.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro inesperado na API.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Erro interno.", "Nao foi possivel concluir a operacao.");
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
