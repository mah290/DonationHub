using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Donations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Tests
{
    internal static class TestData
    {
        public static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DonationOptions Options = new();   // 1.00 .. 10,000.00

        public static Campaign NewCampaign(decimal goal = 1000m, DateTime? endDate = null) =>
            Campaign.Create("حملة", "Campaign", "", "", goal,
                endDate ?? Now.AddDays(10), Guid.NewGuid(), Guid.NewGuid(), Now);

        public static Donation NewDonation(Campaign campaign, decimal amount = 100m) =>
            Donation.Create(campaign, Guid.NewGuid(), amount, Options, Now);
    }
}
