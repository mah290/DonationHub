using DonationHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Interceptors
{
    internal sealed class SoftDeleteInterceptor(TimeProvider clock) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
        {
            ConvertDeletesToSoftDeletes(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
        {
            ConvertDeletesToSoftDeletes(eventData.Context);
            return base.SavingChangesAsync(eventData, result, ct);
        }

        private void ConvertDeletesToSoftDeletes(DbContext? context)
        {
            if (context is null) return;

            var now = clock.GetUtcNow().UtcDateTime;

            var deleted = context.ChangeTracker
                .Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in deleted)
            {
                entry.State = EntityState.Modified;      // prevent physical deletion
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAtUtc = now;
            }
        }
    }
}
