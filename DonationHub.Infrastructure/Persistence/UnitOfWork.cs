using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await context.SaveChangesAsync(ct);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex);
            }
        }
        
    }
}
