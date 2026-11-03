using System.Security.Claims;

namespace DonationHub.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("sub"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Missing or invalid 'sub' claim.");
    }
}
