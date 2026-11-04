using DonationHub.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal sealed class AsyncQueryExecutor : IAsyncQueryExecutor
    {
        public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct = default)
        {
            return query.FirstOrDefaultAsync(ct);
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct = default)
        {
            return query.ToListAsync(ct);
        }
    }
}
