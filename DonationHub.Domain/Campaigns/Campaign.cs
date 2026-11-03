using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Campaigns
{
    public sealed class Campaign : Entity, ISoftDeletable
    {
        public string TitleAr { get; private set; } = default!;
        public string TitleEn { get; private set; } = default!;
        public string DescriptionAr { get; private set; } = default!;
        public string DescriptionEn { get; private set; } = default!;
        public decimal GoalAmount { get; private set; }
        public decimal RaisedAmount { get; private set; }
        public DateTime EndDate { get; private set; }
        public Guid CategoryId { get; private set; }
        public string? BannerFileName { get; private set; }
        public CampaignStatus Status { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public byte[] RowVersion { get; private set; } = [];
        public DateTime? DeletedAtUtc { get; private set; }
        //Explicit implementation
        bool ISoftDeletable.IsDeleted { get => IsDeleted; set => IsDeleted = value; }
        DateTime? ISoftDeletable.DeletedAtUtc 
        {
            get => DeletedAtUtc;
            set => DeletedAtUtc = value;
        }

        private Campaign() { }

        public static Campaign Create(
            string titleAr, string titleEn, string descriptionAr, string descriptionEn,
            decimal goalAmount, DateTime endDate, Guid categoryId, Guid createdByUserId, DateTime now)
        {
            EnsureGoalIsPositive(goalAmount);          // Invariant #1
            EnsureEndDateInFuture(endDate, now);       // Invariant #2

            var campaign = new Campaign
            {
                Id = Guid.NewGuid(),
                GoalAmount = goalAmount,
                RaisedAmount = 0m,
                EndDate = endDate,
                CategoryId = categoryId,
                Status = CampaignStatus.Active,
                CreatedByUserId = createdByUserId,
                CreatedAt = now
            };
            campaign.ApplyDetails(titleAr, titleEn, descriptionAr, descriptionEn, categoryId, now);
            return campaign;
        }

        public void UpdateDetails(
            string titleAr, string titleEn, string descriptionAr, string descriptionEn,
            Guid categoryId, DateTime now)
            => ApplyDetails(titleAr, titleEn, descriptionAr, descriptionEn, categoryId, now);

        // Invariants #1 and #7
        public void ChangeGoal(decimal newGoal, DateTime now)
        {
            EnsureGoalIsPositive(newGoal);
            if (newGoal < RaisedAmount)
                throw new DomainException("campaign.goal_below_raised",
                    "Goal cannot be lowered below the amount already raised.");

            GoalAmount = newGoal;
            UpdatedAt = now;
        }

        // Invariant #2
        public void ChangeEndDate(DateTime newEndDate, DateTime now)
        {
            EnsureEndDateInFuture(newEndDate, now);
            EndDate = newEndDate;
            UpdatedAt = now;
        }

        public void SetBanner(string? fileName, DateTime now)
        {
            BannerFileName = fileName;
            UpdatedAt = now;
        }

        // Invariant #3
        public void EnsureCanAcceptDonation(DateTime now)
        {
            if (IsDeleted || Status != CampaignStatus.Active || EndDate <= now)
                throw new DomainException("campaign.not_accepting_donations",
                    "Campaign is not accepting donations.");
        }

        // Invariants #5 and #6.
        // 'internal' => ONLY Donation.MarkSucceeded can call it,
        // so RaisedAmount can't change any other way.
        internal void RegisterSucceededDonation(decimal amount, DateTime now)
        {
            RaisedAmount += amount;
            UpdatedAt = now;

            if (Status == CampaignStatus.Active && RaisedAmount >= GoalAmount)
                Status = CampaignStatus.Completed;
        }

        // Invariant #8: only an Active campaign can leave Active.
        public void MarkExpired(DateTime now)
        {
            if (Status != CampaignStatus.Active)
                throw new DomainException("campaign.final_state",
                    "A completed or expired campaign cannot change status.");
            if (EndDate > now)
                throw new DomainException("campaign.not_ended", "Campaign has not ended yet.");

            Status = CampaignStatus.Expired;
            UpdatedAt = now;
        }

        // Invariant #9: soft delete is the ONLY delete path the domain exposes.
        public void SoftDelete(DateTime now)
        {
            if (IsDeleted)
                throw new DomainException("campaign.already_deleted", "Campaign is already deleted.");

            IsDeleted = true;
            DeletedAtUtc = now;   // Added in task 5 
            UpdatedAt = now;
        }

        private void ApplyDetails(
            string titleAr, string titleEn, string descriptionAr, string descriptionEn,
            Guid categoryId, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(titleAr) || string.IsNullOrWhiteSpace(titleEn))
                throw new DomainException("campaign.title_required", "Arabic and English titles are required.");

            TitleAr = titleAr.Trim();
            TitleEn = titleEn.Trim();
            DescriptionAr = descriptionAr?.Trim() ?? string.Empty;
            DescriptionEn = descriptionEn?.Trim() ?? string.Empty;
            CategoryId = categoryId;
            UpdatedAt = now;
        }

        private static void EnsureGoalIsPositive(decimal goal)
        {
            if (goal <= 0)
                throw new DomainException("campaign.invalid_goal", "Goal amount must be greater than zero.");
        }

        private static void EnsureEndDateInFuture(DateTime endDate, DateTime now)
        {
            if (endDate <= now)
                throw new DomainException("campaign.invalid_end_date", "End date must be in the future.");
        }
    }
}
