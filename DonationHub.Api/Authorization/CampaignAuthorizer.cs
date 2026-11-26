using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Campaigns;
using Microsoft.AspNetCore.Authorization;

namespace DonationHub.Api.Authorization
{
    internal sealed class CampaignAuthorizer(IHttpContextAccessor http, IAuthorizationService authorization) : ICampaignAuthorizer
    {
        public async Task<bool> CanManageAsync(Campaign campaign, CancellationToken cancellationToken = default)
        {
            var user = http.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true) return false;

            var result = await authorization.AuthorizeAsync(user, campaign, new CampaignOwnershipRequirement());
            return result.Succeeded;
        }
    }
}
