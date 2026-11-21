namespace DonationHub.Application.Common;

public static class DateTimeExtensions
{
    // Converts a DateTime to UTC, preserving the same point in time.
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}