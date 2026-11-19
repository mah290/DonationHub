using DonationHub.Application.Common.Results;

namespace DonationHub.Application.Features.Donations;

public static class DonationErrors
{
    public static Error NotFound() =>
        new("Donation.NotFound", "Donation was not found.", ErrorType.NotFound);

    public static Error AmountOutOfRange(decimal min, decimal max) =>
        new("Donation.AmountOutOfRange",
            $"Amount must be between {min} and {max}.", ErrorType.UnprocessableEntity);

    public static Error InvalidState() =>
        new("Donation.InvalidState", "Donation is already in a final state.", ErrorType.Conflict);
}