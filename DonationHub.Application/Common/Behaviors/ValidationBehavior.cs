using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DonationHub.Application.Common.Behaviors
{
    internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : Result
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if(!validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(validators.Select(v=>v.ValidateAsync(context,cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

            if(failures.Count() == 0)
                return await next();
            var error = new Error(
            "Validation.Failed",
            string.Join(" ", failures.Select(f => f.ErrorMessage).Distinct()),
            ErrorType.Validation);

            return CreateFailure(error);


        }

        private static TResponse CreateFailure(Error error)
        {
            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(error);

            // Result<T>.Failure(error)
            var failure = typeof(TResponse).GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

            return (TResponse)failure.Invoke(null, [error])!;
        }
    }
}
