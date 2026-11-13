using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Results
{
    internal static class ResultFactory
    {
        public static TResponse Failure<TResponse>(Error error) where TResponse : Result
        {
            if(typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(error);
            var failure = typeof(TResponse).GetMethod(
            nameof(Result.Failure),
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

            return (TResponse)failure.Invoke(null, [error])!;
        }
    }
}
