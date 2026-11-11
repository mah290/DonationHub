using DonationHub.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace DonationHub.Api.ExceptionHandling;

internal sealed class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException domain)
            return false;   

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status400BadRequest,
                Title = domain.Code,
                Detail = domain.Message,
                Extensions = { ["code"] = domain.Code }
            }
        });
    }
}