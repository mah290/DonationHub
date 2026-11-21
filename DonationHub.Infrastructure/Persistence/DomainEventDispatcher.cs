using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Events;
using DonationHub.Domain.Common;
using MediatR;

namespace DonationHub.Infrastructure.Persistence;

internal sealed class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var type = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(type, domainEvent)!;

            await publisher.Publish(notification, ct);
        }
    }
}