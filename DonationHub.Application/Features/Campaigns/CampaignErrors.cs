using DonationHub.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Campaigns
{
    public static class CampaignErrors
    {
        public static Error ConcurrencyConflict() =>
            new ("Campaign.ConcurrencyConflict", "The campaign has been modified by another process. Please reload and try again.", ErrorType.Conflict);
    }
}
