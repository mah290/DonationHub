using DonationHub.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public interface ICampaignAuthorizer
    {
        Task<bool> CanManageAsync(Campaign campaign, CancellationToken cancellationToken = default);
    }
}
