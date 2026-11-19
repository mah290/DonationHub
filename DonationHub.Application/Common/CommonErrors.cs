namespace DonationHub.Application.Common.Results;

public static class CommonErrors
{
    public static Error ValidationFailed(string message) =>
        new("Validation.Failed", message, ErrorType.Validation);

    public static Error RateLimited() =>
        new("Common.RateLimited", "Rate limit exceeded. Try again later.", ErrorType.TooManyRequests);

    public static Error ValidationFailed(IReadOnlyDictionary<string, string[]> errors) =>
    new("Validation.Failed", "One or more validation errors occurred.",
        ErrorType.Validation, errors);
}