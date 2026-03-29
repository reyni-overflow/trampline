using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Trampline.Core.Exceptions;

namespace Trampline.Web.Middlewares;

public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {ExceptionType} - {Message}", exception.GetType().Name, exception.Message);

        if (!context.Response.HasStarted)
        {
            int statusCode = StatusCodes.Status500InternalServerError;

            switch (exception)
            {
                case JsonException: statusCode = StatusCodes.Status400BadRequest; break;
                case TaskCanceledException: statusCode = StatusCodes.Status400BadRequest; break;
                case DomainException: statusCode = StatusCodes.Status400BadRequest; break;
                case UnauthorizedAccessException: statusCode = StatusCodes.Status401Unauthorized; break;
                case KeyNotFoundException: statusCode = StatusCodes.Status404NotFound; break;
                case ArgumentException: statusCode = StatusCodes.Status400BadRequest; break;
                case TimeoutException: statusCode = StatusCodes.Status408RequestTimeout; break;
                case NotImplementedException: statusCode = StatusCodes.Status501NotImplemented; break;
            }
            var problemDetails = CreateProblemDetails(context, exception, statusCode);
            var json = ToJson(problemDetails);
            const string contentType = "application/problem+json";
            context.Response.ContentType = contentType;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(json, cancellationToken);
        }

        return true;
    }

    private ProblemDetails CreateProblemDetails(in HttpContext context, in Exception exception, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);

        if (string.IsNullOrEmpty(reasonPhrase))
            reasonPhrase = UnhandledExceptionMsg;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = reasonPhrase
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id;

        return problemDetails;
    }

    private string ToJson(in ProblemDetails problemDetails)
    {
        try
        {
            return JsonSerializer.Serialize(problemDetails, SerializerOptions);
        }
        catch (Exception ex)
        {
            const string msg = "An exception has occurred while serializing error to JSON";
            logger.LogError(ex, msg);
        }

        return string.Empty;
    }
}
