namespace DonationHub.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}