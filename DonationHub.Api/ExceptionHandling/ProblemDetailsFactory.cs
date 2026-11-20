using DonationHub.Api.Extensions;
using DonationHub.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.ExceptionHandling
{
    internal static class ProblemFactory
    {
        public static ProblemDetails Create(HttpContext http, Error error)
        {
            var status = error.Type.ToStatusCode();
            ProblemDetails problem = error.ValidationErrors is { Count :> 0 } errors
                ? new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value))
                : new ProblemDetails();
            Fill(http, problem, status, error.Message, error.Code);
            return problem;


        }

        public static ProblemDetails Create(HttpContext http, int status, string title, string code)
        {
            var problem = new ProblemDetails();
            Fill(http, problem, status, title, code);
            return problem;
        }

        public static void Fill(HttpContext http, ProblemDetails problem , int status , string title , string code)
        {
            problem.Type = $"https://httpstatuses.io/{status}";
            problem.Title = title;
            problem.Status = status;
            problem.Extensions["code"] = code;
            problem.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? http.TraceIdentifier;
        }
    }
}
