using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Campaigns
{
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        void SetExpectedRowVersion(Campaign campaign, byte[] rowVersion);
    }
}
