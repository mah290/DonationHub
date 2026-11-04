using DonationHub.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal class GenericRepository<T> (AppDbContext context):IGenericRepository<T> where T : Entity  
    {
        protected readonly AppDbContext Context = context; // to access the database context in derived classes
        protected readonly DbSet<T> DbSet = context.Set<T>();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await DbSet.FindAsync([id], ct);

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await DbSet.AddAsync(entity, ct);

        public void Update(T entity) => DbSet.Update(entity);

        public void Remove(T entity) => DbSet.Remove(entity);
    }
}
