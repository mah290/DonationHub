using DonationHub.Application.Features.Campaigns;
using DonationHub.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal sealed class CampaignRepository(AppDbContext context) : GenericRepository<Campaign>(context), ICampaignRepository
    {
        public void SetExpectedRowVersion(Campaign campaign, byte[] rowVersion)
        {
            Context.Entry(campaign).Property(c => c.RowVersion).OriginalValue = rowVersion;

        }
    }
}
