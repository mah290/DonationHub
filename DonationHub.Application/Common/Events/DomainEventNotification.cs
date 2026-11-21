using DonationHub.Domain.Common;
using MediatR;

namespace DonationHub.Application.Common.Events;

public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;