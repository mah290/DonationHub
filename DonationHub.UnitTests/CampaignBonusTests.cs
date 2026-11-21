using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace DonationHub.UnitTests;

public class CampaignCancellationTests
{
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    [Fact]
    public void Cancel_ActiveCampaign_BecomesCancelledAndRaisesEvent()
    {
        var campaign = TestData.NewCampaign(Now);

        campaign.Cancel(Now);

        campaign.Status.Should().Be(CampaignStatus.Cancelled);
        campaign.DomainEvents.Should().ContainSingle(e => e is CampaignCancelledEvent);
    }

    [Theory]
    [InlineData(CampaignStatus.Completed)]
    [InlineData(CampaignStatus.Expired)]
    [InlineData(CampaignStatus.Cancelled)]
    public void Cancel_FromFinalState_ThrowsNotActive(CampaignStatus status)
    {
        var campaign = TestData.NewCampaign(Now, goal: 100m);
        switch (status)
        {
            case CampaignStatus.Completed: TestData.Donate(campaign, 100m, Now); break;
            case CampaignStatus.Expired: campaign.MarkExpired(Now.AddDays(11)); break;
            case CampaignStatus.Cancelled: campaign.Cancel(Now); break;
        }

        var act = () => campaign.Cancel(Now);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.NotActive");
    }

    [Fact]
    public void Cancelled_Campaign_CannotBeUpdatedOrAcceptDonations()
    {
        var campaign = TestData.NewCampaign(Now);
        campaign.Cancel(Now);

        var update = () => campaign.Update("ع", "T", "", "", campaign.CategoryId, 500m, campaign.EndDate, Now);
        var accept = () => campaign.EnsureCanAcceptDonation(Now);

        update.Should().Throw<DomainException>().Which.Code.Should().Be("Campaign.NotActive");
        accept.Should().Throw<DomainException>();
    }
}

public class CampaignDomainEventTests
{
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    [Fact]
    public void ReachingGoal_RaisesCompletedEventOnce()
    {
        var campaign = TestData.NewCampaign(Now, goal: 150m);

        TestData.Donate(campaign, 100m, Now);
        campaign.DomainEvents.Should().BeEmpty();           

        TestData.Donate(campaign, 50m, Now);

        campaign.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CampaignCompletedEvent>()
            .Which.RaisedAmount.Should().Be(150m);
    }

    [Fact]
    public void LateSucceededDonationAfterCompletion_DoesNotRaiseAnotherEvent()
    {
        // Arrange
        var campaign = TestData.NewCampaign(Now, goal: 100m);
        var options = new DonationHub.Domain.Donations.DonationOptions();
        var first = DonationHub.Domain.Donations.Donation.Create(campaign, Guid.NewGuid(), 100m, options, Now);
        var late = DonationHub.Domain.Donations.Donation.Create(campaign, Guid.NewGuid(), 10m, options, Now);

        // Act
        first.MarkSucceeded(campaign, Now);    
        late.MarkSucceeded(campaign, Now);     

        // Assert
        campaign.RaisedAmount.Should().Be(110m);
        campaign.Status.Should().Be(CampaignStatus.Completed);
        campaign.DomainEvents.OfType<CampaignCompletedEvent>().Should().HaveCount(1);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesTheList()
    {
        var campaign = TestData.NewCampaign(Now);
        campaign.Cancel(Now);

        campaign.ClearDomainEvents();

        campaign.DomainEvents.Should().BeEmpty();
    }
}

public class CampaignSpecificationTests
{
    private readonly DateTime _now = TestData.Start.UtcDateTime;

    private Campaign Make(Guid? categoryId = null, string title = "Water Well")
    {
        var c = Campaign.Create("بئر", title, "", "", 1000m, _now.AddDays(10),
            categoryId ?? Guid.NewGuid(), Guid.NewGuid(), _now);
        return c;
    }

    [Fact]
    public void Active_MatchesOnlyActiveCampaigns()
    {
        var active = Make();
        var cancelled = Make();
        cancelled.Cancel(_now);

        CampaignSpecifications.Active().IsSatisfiedBy(active).Should().BeTrue();
        CampaignSpecifications.Active().IsSatisfiedBy(cancelled).Should().BeFalse();
    }

    [Fact]
    public void And_RequiresBothConditions()
    {
        var categoryId = Guid.NewGuid();
        var spec = CampaignSpecifications.Active().And(CampaignSpecifications.InCategory(categoryId));

        spec.IsSatisfiedBy(Make(categoryId)).Should().BeTrue();
        spec.IsSatisfiedBy(Make(Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public void Or_AcceptsEitherCondition()
    {
        var done = Make();
        TestData.Donate(done, 1000m, _now);
        var spec = CampaignSpecifications.Completed().Or(CampaignSpecifications.TitleContains("Well"));

        spec.IsSatisfiedBy(done).Should().BeTrue();         
        spec.IsSatisfiedBy(Make()).Should().BeTrue();       
        spec.IsSatisfiedBy(Make(title: "School")).Should().BeFalse();
    }

    [Fact]
    public void Combined_TranslatesToAQueryableFilter()
    {
        var categoryId = Guid.NewGuid();
        var items = new[] { Make(categoryId), Make(), Make(categoryId) };
        var spec = CampaignSpecifications.Active().And(CampaignSpecifications.InCategory(categoryId));

        var result = items.AsQueryable().Where(spec.ToExpression()).ToList();

        result.Should().HaveCount(2);
    }
}