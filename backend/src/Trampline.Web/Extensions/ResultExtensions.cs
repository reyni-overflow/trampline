using Microsoft.AspNetCore.Mvc;
using Trampline.Shared.Results;

namespace Trampline.Web.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        var errors = result.Errors.ToDictionary(
            e => e.Field,
            e => new[] { e.Message });

        var problems = result.ErrorCode switch
        {
            401 => new ValidationProblemDetails(errors) { Status = 401, Title = "Unauthorized" },
            403 => new ProblemDetails { Status = 403, Title = "Forbidden", Detail = result.Errors.FirstOrDefault()?.Message },
            404 => new ProblemDetails { Status = 404, Title = "Not Found", Detail = result.Errors.FirstOrDefault()?.Message },
            _ => new ValidationProblemDetails(errors) { Status = result.ErrorCode ?? 400, Title = "One or more errors occurred" }
        };

        return new ObjectResult(problems)
        {
            StatusCode = problems.Status ?? 400,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        var errors = result.Errors.ToDictionary(
            e => e.Field,
            e => new[] { e.Message });

        var problems = result.ErrorCode switch
        {
            401 => new ValidationProblemDetails(errors)
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = result.Errors.FirstOrDefault()?.Message ?? "Access denied"
            },
            404 => new ProblemDetails()
            {
                Status = 404,
                Title = "Not Found",
                Detail = result.Errors.FirstOrDefault()?.Message ?? "Resource not found"
            },
            _ => new ValidationProblemDetails(errors)
            {
                Status = result.ErrorCode ?? 400,
                Title = "One or more errors occurred",
                Detail = result.Errors.FirstOrDefault()?.Message ?? "See errors for details"
            }
        };

        return new ObjectResult(problems)
        {
            StatusCode = problems.Status ?? 400,
            ContentTypes = { "application/problem+json" }
        };
    }
}