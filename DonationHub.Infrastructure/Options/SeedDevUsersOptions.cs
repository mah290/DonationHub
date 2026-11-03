using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Options
{
    public sealed class SeedDevUsersOptions
    {
        public const string SectionName = "SeedDevUsers";

        public string CampaignManagerEmail { get; set; } = string.Empty;
        public string CampaignManagerPassword { get; set; } = string.Empty;
        public string DonorEmail { get; set; } = string.Empty;
        public string DonorPassword { get; set; } = string.Empty;
    }
}
