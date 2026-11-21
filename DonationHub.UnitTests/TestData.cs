using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Donations;
using Microsoft.Extensions.Time.Testing;

namespace DonationHub.UnitTests;

internal static class TestData
{
    public static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static FakeTimeProvider NewClock() => new(Start);

    public static Campaign NewCampaign(DateTime now, decimal goal = 1000m, int daysToEnd = 10) =>
        Campaign.Create("حملة", "Campaign", "وصف", "Description",
            goal, now.AddDays(daysToEnd), Guid.NewGuid(), Guid.NewGuid(), now);

    public static Donation Donate(Campaign campaign, decimal amount, DateTime now)
    {
        var donation = Donation.Create(campaign, Guid.NewGuid(), amount, new DonationOptions(), now);
        donation.MarkSucceeded(campaign, now);
        return donation;
    }
}