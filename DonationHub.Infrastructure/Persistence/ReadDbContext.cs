using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Categories;
using DonationHub.Domain.Donations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal sealed class ReadDbContext(AppDbContext context) : IReadDbContext
    {
        public IQueryable<Category> Categories => context.Categories.AsNoTracking();
        public IQueryable<Campaign> Campaigns => context.Campaigns.AsNoTracking();
        public IQueryable<Donation> Donations => context.Donations.AsNoTracking();
    }
}
