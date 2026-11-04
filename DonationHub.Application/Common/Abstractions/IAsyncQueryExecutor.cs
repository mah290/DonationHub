using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public interface IAsyncQueryExecutor
    {
        Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct = default);
    }
}
