using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Categories;
using DonationHub.Domain.Donations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public interface IReadDbContext
    {
        IQueryable<Category> Categories { get; }
        IQueryable<Campaign> Campaigns { get; }
        IQueryable<Donation> Donations { get; }
    }
}
