using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Common;
using Microsoft.Extensions.Time.Testing;

namespace DonationHub.Domain.Tests;

public class CampaignReadModelTests
{
    [Theory]
    [InlineData(50, 200, 25)]
    [InlineData(200, 200, 100)]
    [InlineData(300, 200, 100)]   // min(100, ...)
    [InlineData(0, 200, 0)]
    public void Progress_IsCappedAt100(decimal raised, decimal goal, decimal expected)
        => Assert.Equal(expected, CampaignReadModel.Progress(raised, goal));

    [Fact]
    public void DaysLeft_NeverNegative_AndFollowsTimeProvider()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var end = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(10, CampaignReadModel.DaysLeft(end, clock.GetUtcNow().UtcDateTime));

        clock.Advance(TimeSpan.FromDays(30));
        Assert.Equal(0, CampaignReadModel.DaysLeft(end, clock.GetUtcNow().UtcDateTime));
    }

    [Theory]
    [InlineData("Campaign.EndDateInPast", ErrorType.UnprocessableEntity)]
    [InlineData("Campaign.GoalBelowRaised", ErrorType.UnprocessableEntity)]
    [InlineData("Campaign.NotActive", ErrorType.Conflict)]
    [InlineData("Campaign.InvalidGoal", ErrorType.Validation)]
    public void DomainErrorMap_MapsCampaignCodes(string code, ErrorType type)
    {
        var error = DomainErrorMap.Map(new DomainException(code, "x"));

        Assert.NotNull(error);
        Assert.Equal(type, error!.Type);
    }
}