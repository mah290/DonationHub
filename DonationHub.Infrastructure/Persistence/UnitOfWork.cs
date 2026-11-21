using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Exceptions;
using DonationHub.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace DonationHub.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        int rows;
        try
        {
            rows = await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(ex);
        }
        var entities = context.ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        if (events.Count > 0)
            await dispatcher.DispatchAsync(events, ct);

        return rows;
    }
}