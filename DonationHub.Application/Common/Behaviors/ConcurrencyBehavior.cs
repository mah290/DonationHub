using DonationHub.Application.Common.Exceptions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Campaigns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Behaviors
{
    internal sealed class ConcurrencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Result
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (ConcurrencyException)
            {
                return ResultFactory.Failure<TResponse>(CampaignErrors.ConcurrencyConflict());
            }
        }
    }
}
