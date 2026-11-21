using DonationHub.Domain.Common;

namespace DonationHub.Application.Common.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct = default);
}