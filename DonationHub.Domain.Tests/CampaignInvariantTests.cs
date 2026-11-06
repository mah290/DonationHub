using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using static DonationHub.Domain.Tests.TestData;

namespace DonationHub.Domain.Tests;

public class CampaignInvariantTests
{
    // Invariant 1
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveGoal_Throws(decimal goal)
    {
        var ex = Assert.Throws<DomainException>(() => NewCampaign(goal));
        Assert.Equal("campaign.invalid_goal", ex.Code);
    }

    // Invariant 2
    [Fact]
    public void Create_WithPastEndDate_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => NewCampaign(endDate: Now.AddDays(-1)));
        Assert.Equal("campaign.invalid_end_date", ex.Code);
    }

    // Invariant 2
    [Fact]
    public void ChangeEndDate_ToPast_Throws()
    {
        var campaign = NewCampaign();
        var ex = Assert.Throws<DomainException>(() => campaign.ChangeEndDate(Now.AddDays(-1), Now));
        Assert.Equal("campaign.invalid_end_date", ex.Code);
    }

    // Invariant 3
    [Fact]
    public void EnsureCanAcceptDonation_AfterEndDate_Throws()
    {
        var campaign = NewCampaign();
        Assert.Throws<DomainException>(() => campaign.EnsureCanAcceptDonation(Now.AddDays(11)));
    }

    // Invariant 3
    [Fact]
    public void EnsureCanAcceptDonation_WhenCompleted_Throws()
    {
        var campaign = NewCampaign(goal: 100m);
        var donation = NewDonation(campaign, 100m);
        donation.MarkSucceeded(campaign, Now);   // الحملة بقت Completed

        Assert.Throws<DomainException>(() => campaign.EnsureCanAcceptDonation(Now));
    }

    // Invariant 3
    [Fact]
    public void EnsureCanAcceptDonation_WhenDeleted_Throws()
    {
        var campaign = NewCampaign();
        campaign.SoftDelete(Now);

        Assert.Throws<DomainException>(() => campaign.EnsureCanAcceptDonation(Now));
    }

    // Invariant 7
    [Fact]
    public void ChangeGoal_BelowRaisedAmount_Throws()
    {
        var campaign = NewCampaign(goal: 1000m);
        NewDonation(campaign, 500m).MarkSucceeded(campaign, Now);

        var ex = Assert.Throws<DomainException>(() => campaign.ChangeGoal(400m, Now));
        Assert.Equal("campaign.goal_below_raised", ex.Code);
    }

    [Fact]
    public void ChangeGoal_AboveRaisedAmount_Succeeds()
    {
        var campaign = NewCampaign(goal: 1000m);
        NewDonation(campaign, 500m).MarkSucceeded(campaign, Now);

        campaign.ChangeGoal(500m, Now);

        Assert.Equal(500m, campaign.GoalAmount);
    }

    // Invariant 8
    [Fact]
    public void MarkExpired_WhenCompleted_Throws()
    {
        var campaign = NewCampaign(goal: 100m);
        NewDonation(campaign, 100m).MarkSucceeded(campaign, Now);

        var ex = Assert.Throws<DomainException>(() => campaign.MarkExpired(Now.AddDays(11)));
        Assert.Equal("campaign.final_state", ex.Code);
    }

    [Fact]
    public void MarkExpired_AfterEndDate_SetsExpired()
    {
        var campaign = NewCampaign();
        campaign.MarkExpired(Now.AddDays(11));

        Assert.Equal(CampaignStatus.Expired, campaign.Status);
    }

    // Invariant 9
    [Fact]
    public void SoftDelete_SetsFlagsAndCannotRepeat()
    {
        var campaign = NewCampaign();
        campaign.SoftDelete(Now);

        Assert.True(campaign.IsDeleted);
        Assert.Equal(Now, campaign.DeletedAtUtc);
        Assert.Throws<DomainException>(() => campaign.SoftDelete(Now));
    }
}
