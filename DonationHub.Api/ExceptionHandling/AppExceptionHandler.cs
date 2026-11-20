using DonationHub.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.ExceptionHandling;

internal sealed class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem;

        if (exception is DomainException domain)
        {
            // Log the domain exception as a warning, since it represents a known business rule violation.
            logger.LogWarning(exception, "Domain rule violated: {Code}", domain.Code);

            problem = ProblemFactory.Create(
                httpContext, StatusCodes.Status422UnprocessableEntity, domain.Message, domain.Code);
        }
        else
        {
            // Log the unhandled exception as an error, since it represents an unexpected failure in the application.
            logger.LogError(exception,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}",
                System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier,
                httpContext.Request.Path);

            problem = ProblemFactory.Create(
                httpContext, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.", "Common.InternalError");
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, "application/problem+json", ct);

        return true;
    }
}