using System.Net;
using System.Text.Json;
using PokemonWordle.Api.Exceptions;

namespace PokemonWordle.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            GameNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            GameAlreadyCompleteException => (HttpStatusCode.Conflict, exception.Message),
            InvalidPokemonException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            status = (int)statusCode,
            message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
