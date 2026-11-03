using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace DonationHub.Domain.Donations
{
    public sealed class Donation : Entity
    {
        public Guid CampaignId { get; private set; }
        public Guid DonorUserId { get; private set; }
        public decimal Amount { get; private set; }
        public DonationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? FailureReason { get; private set; }
        public string? ReceiptNumber { get; private set; }
        public string? ReceiptFileName { get; private set; }
        private Donation() { }

        public static Donation Create(
            Campaign campaign, Guid donorUserId, decimal amount, DonationOptions options, DateTime now)
        {
            campaign.EnsureCanAcceptDonation(now);   // Invariant #3

            // Invariant #4
            if (amount < options.MinAmount || amount > options.MaxAmount)
                throw new DomainException("donation.amount_out_of_range",
                    $"Amount must be between {options.MinAmount} and {options.MaxAmount}.");
            if (Math.Round(amount, 2) != amount)
                throw new DomainException("donation.invalid_precision",
                    "Amount cannot have more than 2 decimal places.");

            return new Donation
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                DonorUserId = donorUserId,
                Amount = amount,
                Status = DonationStatus.Pending,
                CreatedAt = now
            };
        }

        // Invariants #5, #6, #8
        public void MarkSucceeded(Campaign campaign, DateTime now)
        {
            EnsurePending();

            if (campaign.Id != CampaignId)
                throw new DomainException("donation.campaign_mismatch",
                    "Donation does not belong to this campaign.");

            Status = DonationStatus.Succeeded;
            CompletedAt = now;
            campaign.RegisterSucceededDonation(Amount, now);
        }

        // Invariant #8
        public void MarkFailed(string reason, DateTime now)
        {
            EnsurePending();

            Status = DonationStatus.Failed;
            FailureReason = reason;
            CompletedAt = now;
        }

        public void AttachReceipt(string receiptNumber, string receiptFileName)
        {
            if (Status != DonationStatus.Succeeded)
                throw new DomainException("donation.receipt_requires_success",
                    "Receipt is only available for succeeded donations.");

            ReceiptNumber = receiptNumber;
            ReceiptFileName = receiptFileName;
        }

        private void EnsurePending()
        {
            if (Status != DonationStatus.Pending)
                throw new DomainException("donation.final_state",
                    "A succeeded or failed donation cannot change.");
        }
    }
}
