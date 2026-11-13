using DonationHub.Domain.Common;
using Microsoft.Extensions.Time.Testing;
using static DonationHub.Domain.Tests.TestData;

namespace DonationHub.Domain.Tests;

public class ExpiryTests
{
    [Fact]
    public void Campaign_StopsAcceptingDonations_AfterEndDate()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(Now, TimeSpan.Zero));
        var campaign = NewCampaign(endDate: Now.AddDays(10));

        // قبل الانتهاء: مفيش exception
        campaign.EnsureCanAcceptDonation(clock.GetUtcNow().UtcDateTime);

        clock.Advance(TimeSpan.FromDays(11));   // نقدّم الوقت من غير ما نستنى

        Assert.Throws<DomainException>(() =>
            campaign.EnsureCanAcceptDonation(clock.GetUtcNow().UtcDateTime));
    }
}