using DonationHub.Application.Common.Results;

namespace DonationHub.Application.Features.Campaigns;

public static class CampaignErrors
{
    public static Error NotFound() =>
        new("Campaign.NotFound", "Campaign was not found.", ErrorType.NotFound);

    public static Error NotActive() =>
        new("Campaign.NotActive", "Campaign is completed or expired.", ErrorType.Conflict);

    public static Error GoalBelowRaised() =>
        new("Campaign.GoalBelowRaised",
            "Goal amount cannot be lower than the amount already raised.",
            ErrorType.UnprocessableEntity);

    public static Error EndDateInPast() =>
        new("Campaign.EndDateInPast", "End date must be in the future.", ErrorType.UnprocessableEntity);

    public static Error ConcurrencyConflict() =>
        new("Campaign.ConcurrencyConflict",
            "The campaign was modified by someone else. Reload it and try again.",
            ErrorType.Conflict);
}