using DonationHub.Domain.Common;

namespace DonationHub.Domain.Campaigns;

public sealed record CampaignCompletedEvent(Guid CampaignId, decimal RaisedAmount, DateTime OccurredAtUtc)
    : IDomainEvent;

public sealed record CampaignCancelledEvent(Guid CampaignId, DateTime OccurredAtUtc) : IDomainEvent;