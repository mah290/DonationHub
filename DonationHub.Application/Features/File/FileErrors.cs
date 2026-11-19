using DonationHub.Application.Common.Results;

namespace DonationHub.Application.Features.Files;

public static class FileErrors
{
    public static Error InvalidType() =>
        new("File.InvalidType", "File type is not allowed.", ErrorType.Validation);

    public static Error TooLarge() =>
        new("File.TooLarge", "File exceeds the maximum allowed size.", ErrorType.PayloadTooLarge);
}