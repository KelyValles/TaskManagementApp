using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TaskManager.Api.Exceptions;

namespace TaskManager.Api.Infrastructure;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            NotFoundException        => ((int)HttpStatusCode.NotFound, "Recurso no encontrado"),
            ConflictException        => ((int)HttpStatusCode.Conflict, "Conflicto"),
            BusinessRuleException    => (422, "Operación no permitida"),
            InvalidJsonException     => ((int)HttpStatusCode.BadRequest, "Datos inválidos"),
            ArgumentException        => ((int)HttpStatusCode.BadRequest, "Datos inválidos"),
            SqlException sql when IsUniqueViolation(sql) => ((int)HttpStatusCode.Conflict, "Valor duplicado"),
            _                        => ((int)HttpStatusCode.InternalServerError, "Error del servidor")
        };

        if (status >= 500)
        {
            _logger.LogError(exception, "Unhandled exception while processing request {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception ({Status}) on {Path}", status, context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 ? "Por favor contacta al administrador." : exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    private static bool IsUniqueViolation(SqlException ex) =>
        ex.Number is 2601 or 2627;
}
