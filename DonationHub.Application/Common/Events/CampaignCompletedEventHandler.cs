using DonationHub.Application.Common.Events;
using DonationHub.Domain.Campaigns;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DonationHub.Application.Features.Campaigns.Events;

internal sealed class CampaignCompletedEventHandler(ILogger<CampaignCompletedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<CampaignCompletedEvent>>
{
    public Task Handle(DomainEventNotification<CampaignCompletedEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        logger.LogInformation("Campaign {CampaignId} completed with {Raised} raised.", e.CampaignId, e.RaisedAmount);
        return Task.CompletedTask;
    }
}

internal sealed class CampaignCancelledEventHandler(ILogger<CampaignCancelledEventHandler> logger)
    : INotificationHandler<DomainEventNotification<CampaignCancelledEvent>>
{
    public Task Handle(DomainEventNotification<CampaignCancelledEvent> notification, CancellationToken ct)
    {
        logger.LogInformation("Campaign {CampaignId} was cancelled.", notification.DomainEvent.CampaignId);
        return Task.CompletedTask;
    }
}