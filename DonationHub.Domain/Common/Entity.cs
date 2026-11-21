using System.ComponentModel.DataAnnotations.Schema;

namespace DonationHub.Domain.Common;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; }

    [NotMapped]   // عشان EF ميحاولش يعمله mapping
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected Entity(Guid id) => Id = id;

    protected Entity() { } // for EF Core

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}