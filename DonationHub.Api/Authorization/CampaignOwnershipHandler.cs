using System.Security.Claims;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace DonationHub.Api.Authorization;

public sealed class CampaignOwnershipHandler
    : AuthorizationHandler<CampaignOwnershipRequirement, Campaign>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, CampaignOwnershipRequirement requirement, Campaign campaign)
    {
        var user = context.User;
        if (user.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.CampaignManager) && Guid.TryParse(user.FindFirstValue("sub"), out var userId) && campaign.CreatedByUserId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;

    }
}