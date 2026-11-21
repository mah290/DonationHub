using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace DonationHub.UnitTests;

public class CampaignDomainTests
{
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    // 1) GoalAmount > 0
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNonPositiveGoal_ThrowsInvalidGoal(decimal goal)
    {
        // Arrange
        var now = Now;

        // Act
        var act = () => TestData.NewCampaign(now, goal);

        // Assert
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.InvalidGoal");
    }

    // 2) EndDate في الماضي عند الإنشاء
    [Fact]
    public void Create_WithEndDateInPast_ThrowsEndDateInPast()
    {
        // Arrange
        var now = Now;

        // Act
        var act = () => Campaign.Create("ع", "T", "", "", 100m, now.AddDays(-1),
            Guid.NewGuid(), Guid.NewGuid(), now);

        // Assert
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.EndDateInPast");
    }

    // 2) تاريخ كان مستقبل وبقى ماضي بعد ما الوقت (المزيّف) اتقدّم
    [Fact]
    public void ChangeEndDate_ToDateThatBecamePastAfterClockAdvance_Throws()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now);
        var proposed = Now.AddDays(5);
        _clock.Advance(TimeSpan.FromDays(6));

        // Act
        var act = () => campaign.ChangeEndDate(proposed, _clock.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.EndDateInPast");
    }

    // 3) Goal تحت RaisedAmount
    [Fact]
    public void Update_GoalBelowRaised_ThrowsGoalBelowRaised()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now, goal: 1000m);
        TestData.Donate(campaign, 500m, Now);

        // Act
        var act = () => campaign.Update("ع", "T", "", "", campaign.CategoryId, 400m, campaign.EndDate, Now);

        // Assert
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.GoalBelowRaised");
        campaign.GoalAmount.Should().Be(1000m);
    }

    [Fact]
    public void Update_GoalEqualToRaised_IsAllowedAndCompletesCampaign()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now, goal: 1000m);
        TestData.Donate(campaign, 500m, Now);

        // Act
        campaign.Update("ع", "T", "", "", campaign.CategoryId, 500m, campaign.EndDate, Now);

        // Assert
        campaign.GoalAmount.Should().Be(500m);
        campaign.Status.Should().Be(CampaignStatus.Completed);
    }

    // 4) الحالات النهائية متغيّرش
    [Theory]
    [InlineData(CampaignStatus.Completed)]
    [InlineData(CampaignStatus.Expired)]
    public void Update_WhenInFinalState_ThrowsNotActive(CampaignStatus finalStatus)
    {
        // Arrange
        var campaign = CampaignIn(finalStatus);

        // Act
        var act = () => campaign.Update("ع", "T", "", "", campaign.CategoryId,
            campaign.GoalAmount, campaign.EndDate, Now);

        // Assert
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.NotActive");
        campaign.Status.Should().Be(finalStatus);
    }

    // 7) الانتقال لـ Completed
    [Fact]
    public void Donation_ReachingGoal_CompletesCampaign()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now, goal: 150m);

        // Act
        TestData.Donate(campaign, 100m, Now);
        var statusAfterFirst = campaign.Status;
        TestData.Donate(campaign, 50m, Now);

        // Assert
        statusAfterFirst.Should().Be(CampaignStatus.Active);
        campaign.Status.Should().Be(CampaignStatus.Completed);
        campaign.RaisedAmount.Should().Be(150m);
    }

    [Fact]
    public void Donation_ExceedingGoal_AlsoCompletesCampaign()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now, goal: 100m);

        // Act
        TestData.Donate(campaign, 150m, Now);

        // Assert
        campaign.Status.Should().Be(CampaignStatus.Completed);
        campaign.RaisedAmount.Should().Be(150m);
    }

    private Campaign CampaignIn(CampaignStatus status)
    {
        var campaign = TestData.NewCampaign(Now, goal: 100m);

        if (status == CampaignStatus.Completed)
            TestData.Donate(campaign, 100m, Now);
        else if (status == CampaignStatus.Expired)
            campaign.MarkExpired(Now.AddDays(11));

        return campaign;
    }
}