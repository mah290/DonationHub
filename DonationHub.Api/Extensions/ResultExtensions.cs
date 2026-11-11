using DonationHub.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace DonationHub.Api.Extensions;

internal static class ResultExtensions
{
    public static IActionResult ToOk<T>(this ControllerBase c, Result<T> result) =>
        result.IsSuccess ? c.Ok(result.Value) : c.ToProblem(result.Error);

    public static IActionResult ToNoContent(this ControllerBase c, Result result) =>
        result.IsSuccess ? c.NoContent() : c.ToProblem(result.Error);

    //This method is used for returning a 201 Created response with the created resource in the body.
    public static IActionResult ToCreated<T>(this ControllerBase c, Result<T> result) =>
        result.IsSuccess ? c.StatusCode(StatusCodes.Status201Created, result.Value) : c.ToProblem(result.Error);

    // 201 + Location header
    public static IActionResult ToCreatedAtRoute<T>(
        this ControllerBase c, Result<T> result, string routeName, Func<T, object> routeValues) =>
        result.IsSuccess
            ? c.CreatedAtRoute(routeName, routeValues(result.Value), result.Value)
            : c.ToProblem(result.Error);

    public static IActionResult ToProblem(this ControllerBase c, Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails { Status = status, Title = error.Code, Detail = error.Message };
        problem.Extensions["code"] = error.Code;

        return c.StatusCode(status, problem);
    }
}