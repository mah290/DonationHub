namespace DonationHub.Api.ExceptionHandling;

internal static class AuthProblemWriter
{
    public static Task WriteAsync(HttpContext http, int status, string title, string code)
    {
        var problem = ProblemFactory.Create(http, status, title, code);

        http.Response.StatusCode = status;
        http.Response.ContentType = "application/problem+json";
        return http.Response.WriteAsJsonAsync(problem, options: null, "application/problem+json");
    }
}