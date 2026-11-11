using DonationHub.Domain.Common;
using DonationHub.Domain.Donations;
using DonationHub.Domain.Enums;
using static DonationHub.Domain.Tests.TestData;

namespace DonationHub.Domain.Tests;

public class DonationInvariantTests
{
    // Invariant 4
    [Theory]
    [InlineData("0.99")]
    [InlineData("10000.01")]
    public void Create_AmountOutsideRange_Throws(string amount)
    {
        var campaign = NewCampaign();
        var ex = Assert.Throws<DomainException>(() => NewDonation(campaign, decimal.Parse(amount)));
        Assert.Equal("donation.amount_out_of_range", ex.Code);
    }

    // Invariant 4 (الحدود نفسها مقبولة)
    [Theory]
    [InlineData("1.00")]
    [InlineData("10000.00")]
    public void Create_AmountAtBoundaries_Succeeds(string amount)
    {
        var donation = NewDonation(NewCampaign(), decimal.Parse(amount));
        Assert.Equal(DonationStatus.Pending, donation.Status);
    }

    // Invariant 4 (الـ options قابلة للتغيير)
    [Fact]
    public void Create_RespectsConfiguredOptions()
    {
        var options = new DonationOptions { MinAmount = 5m, MaxAmount = 50m };
        Assert.Throws<DomainException>(() =>
            Donation.Create(NewCampaign(), Guid.NewGuid(), 4m, options, Now));
    }

    // Invariant 5
    [Fact]
    public void RaisedAmount_DoesNotChange_WhilePendingOrFailed()
    {
        var campaign = NewCampaign();
        var donation = NewDonation(campaign, 100m);
        Assert.Equal(0m, campaign.RaisedAmount);

        donation.MarkFailed("card_declined", Now);
        Assert.Equal(0m, campaign.RaisedAmount);
    }

    // Invariant 5
    [Fact]
    public void RaisedAmount_Increases_OnlyWhenSucceeded()
    {
        var campaign = NewCampaign();
        NewDonation(campaign, 100m).MarkSucceeded(campaign, Now);

        Assert.Equal(100m, campaign.RaisedAmount);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
    }

    // Invariant 6
    [Fact]
    public void Campaign_BecomesCompleted_WhenGoalReached()
    {
        var campaign = NewCampaign(goal: 150m);
        NewDonation(campaign, 100m).MarkSucceeded(campaign, Now);
        Assert.Equal(CampaignStatus.Active, campaign.Status);

        NewDonation(campaign, 50m).MarkSucceeded(campaign, Now);
        Assert.Equal(CampaignStatus.Completed, campaign.Status);
    }

    // Invariant 8
    [Fact]
    public void SucceededDonation_CannotChange()
    {
        var campaign = NewCampaign();
        var donation = NewDonation(campaign);
        donation.MarkSucceeded(campaign, Now);

        Assert.Throws<DomainException>(() => donation.MarkSucceeded(campaign, Now));
        Assert.Throws<DomainException>(() => donation.MarkFailed("x", Now));
    }

    // Invariant 8
    [Fact]
    public void FailedDonation_CannotChange()
    {
        var campaign = NewCampaign();
        var donation = NewDonation(campaign);
        donation.MarkFailed("x", Now);

        Assert.Throws<DomainException>(() => donation.MarkSucceeded(campaign, Now));
        Assert.Equal(0m, campaign.RaisedAmount);
    }

    // Invariant 8
    [Fact]
    public void PaymentAttempt_FinalState_CannotChange()
    {
        var attempt = PaymentAttempt.Create(Guid.NewGuid(), "cs_test_1", 100m, Now);
        attempt.MarkSucceeded("pi_1", Now);

        Assert.Throws<DomainException>(() => attempt.MarkFailed("x", Now));
        Assert.Throws<DomainException>(() => attempt.MarkExpired(Now));
    }

}
