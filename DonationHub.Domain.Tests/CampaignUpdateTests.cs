using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using static DonationHub.Domain.Tests.TestData;

namespace DonationHub.Domain.Tests;

public class CampaignUpdateTests
{
    private static void Update(Domain.Campaigns.Campaign c, decimal? goal = null, DateTime? end = null) =>
        c.Update("ع", "T", "", "", Guid.NewGuid(), goal ?? c.GoalAmount, end ?? c.EndDate, Now);

    [Fact]
    public void Update_Valid_ChangesFields()
    {
        var c = NewCampaign(1000m);
        c.Update("عنوان", "Title", "وصف", "Desc", Guid.NewGuid(), 2000m, Now.AddDays(30), Now);

        Assert.Equal("Title", c.TitleEn);
        Assert.Equal(2000m, c.GoalAmount);
        Assert.Equal(Now.AddDays(30), c.EndDate);
    }

    [Fact]
    public void Update_EndDateInPast_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Update(NewCampaign(), end: Now.AddDays(-1)));
        Assert.Equal("Campaign.EndDateInPast", ex.Code);
    }

    [Fact]
    public void Update_GoalBelowRaised_Throws()
    {
        var c = NewCampaign(1000m);
        NewDonation(c, 500m).MarkSucceeded(c, Now);

        var ex = Assert.Throws<DomainException>(() => Update(c, goal: 400m));
        Assert.Equal("Campaign.GoalBelowRaised", ex.Code);
    }

    [Theory]
    [InlineData(true)]    // Completed
    [InlineData(false)]   // Expired
    public void Update_FinalState_Throws(bool completed)
    {
        var c = NewCampaign(100m);
        if (completed) NewDonation(c, 100m).MarkSucceeded(c, Now);
        else c.MarkExpired(Now.AddDays(11));

        var ex = Assert.Throws<DomainException>(() => Update(c));
        Assert.Equal("Campaign.NotActive", ex.Code);
    }

    [Fact]
    public void Update_UnchangedEndDate_DoesNotRequireFuture()
    {
        var c = NewCampaign(endDate: Now.AddDays(10));
        var later = Now.AddDays(20);   // التاريخ بقى في الماضي بالنسبة لـ now الجديد
        c.Update("ع", "T", "", "", Guid.NewGuid(), 500m, c.EndDate, later);

        Assert.Equal(500m, c.GoalAmount);
    }

    [Fact]
    public void SoftDelete_FinalStateCampaign_IsAllowed()
    {
        var c = NewCampaign(100m);
        NewDonation(c, 100m).MarkSucceeded(c, Now);
        c.SoftDelete(Now);

        Assert.True(c.IsDeleted);
        Assert.Equal(CampaignStatus.Completed, c.Status);
    }
}