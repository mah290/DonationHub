using DonationHub.Application.Common.Exceptions;
using DonationHub.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace DonationHub.Api.ExceptionHandling
{
    internal sealed class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            int status;
            string code;

            switch (exception)
            {
                case AppException app:
                    status = app.Type switch
                    {
                        ErrorType.Validation => StatusCodes.Status400BadRequest,
                        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                        ErrorType.NotFound => StatusCodes.Status404NotFound,
                        ErrorType.Conflict => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status500InternalServerError
                    };
                    code = app.Code;
                    break;

                case DomainException domain:
                    status = StatusCodes.Status400BadRequest;
                    code = domain.Code;
                    break;

                default:
                    return false;   // 500 
            }

            httpContext.Response.StatusCode = status;

            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails =
            {
                Status = status,
                Title = code,
                Detail = exception.Message,
                Extensions = { ["code"] = code }
            }
            });
        }
    }
}
