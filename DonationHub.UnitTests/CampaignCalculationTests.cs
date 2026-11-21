using DonationHub.Application.Features.Campaigns;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace DonationHub.UnitTests;

public class CampaignCalculationTests
{
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    // 5) progressPercent
    [Theory]
    [InlineData(50, 200, 25)]
    [InlineData(200, 200, 100)]
    [InlineData(300, 200, 100)]   // متعدّاش 100
    [InlineData(0, 200, 0)]
    [InlineData(10, 0, 0)]        // goal صفر مينفجرش
    public void Progress_IsCappedAt100(decimal raised, decimal goal, decimal expected)
    {
        // Act
        var progress = CampaignReadModel.Progress(raised, goal);

        // Assert
        progress.Should().Be(expected);
    }

    // 6) daysLeft مع TimeProvider
    [Fact]
    public void DaysLeft_FollowsTimeProvider()
    {
        // Arrange
        var end = Now.AddDays(10);
        var before = CampaignReadModel.DaysLeft(end, Now);

        // Act
        _clock.Advance(TimeSpan.FromDays(4));
        var after = CampaignReadModel.DaysLeft(end, Now);

        // Assert
        before.Should().Be(10);
        after.Should().Be(6);
    }

    [Fact]
    public void DaysLeft_AfterEndDate_IsZeroNeverNegative()
    {
        // Arrange
        var end = Now.AddDays(10);
        _clock.Advance(TimeSpan.FromDays(30));

        // Act
        var daysLeft = CampaignReadModel.DaysLeft(end, Now);

        // Assert
        daysLeft.Should().Be(0);
    }

    [Fact]
    public void DaysLeft_WithPartialDay_RoundsUp()
    {
        // Arrange
        var end = Now.AddHours(36);   // يوم ونص

        // Act
        var daysLeft = CampaignReadModel.DaysLeft(end, Now);

        // Assert
        daysLeft.Should().Be(2);
    }
}