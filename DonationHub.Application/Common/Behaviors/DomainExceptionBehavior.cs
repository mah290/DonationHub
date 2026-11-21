using DonationHub.Application.Common.Results;
using DonationHub.Domain.Common;
using MediatR;

namespace DonationHub.Application.Common.Behaviors;

internal sealed class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next(ct);
        }
        catch (DomainException ex) when (DomainErrorMap.Map(ex) is not null)
        {
            return ResultFactory.Failure<TResponse>(DomainErrorMap.Map(ex)!);
        }
    }
}