using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common;

namespace DonationHub.Application.Common.Behaviors;

internal static class DomainErrorMap
{
    // Maps domain exception codes to application error results.
    public static Error? Map(DomainException ex) => ex.Code switch
    {
        "Campaign.EndDateInPast" => CampaignErrors.EndDateInPast(),
        "Campaign.GoalBelowRaised" => CampaignErrors.GoalBelowRaised(),
        "Campaign.NotActive" => CampaignErrors.NotActive(),
        "Campaign.InvalidGoal" => CommonErrors.ValidationFailed(
            new Dictionary<string, string[]> { ["goalAmount"] = [ex.Message] }),
        _ => null
    };
}