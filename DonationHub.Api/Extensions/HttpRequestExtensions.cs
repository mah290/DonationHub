using DonationHub.Domain.Users;

namespace DonationHub.Api.Extensions;

internal static class HttpRequestExtensions
{
    // Returns the language code (ar or en) based on the Accept-Language header.
    public static string GetLanguage(this HttpRequest request)
    {
        var accepted = request.GetTypedHeaders().AcceptLanguage
            .Where(h => h.Quality is null or > 0)
            .OrderByDescending(h => h.Quality ?? 1.0);

        foreach (var item in accepted)
        {
            var primary = item.Value.Value?.Split('-')[0].ToLowerInvariant();   // ar-EG => ar

            if (primary == Languages.Arabic) return Languages.Arabic;
            if (primary == Languages.English) return Languages.English;
        }

        return Languages.English;
    }
}