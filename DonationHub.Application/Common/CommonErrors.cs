namespace DonationHub.Application.Common.Results;

public static class CommonErrors
{
    public static Error ValidationFailed(string message) =>
        new("Validation.Failed", message, ErrorType.Validation);

    public static Error RateLimited() =>
        new("Common.RateLimited", "Rate limit exceeded. Try again later.", ErrorType.TooManyRequests);
}