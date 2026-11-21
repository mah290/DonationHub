using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;

namespace DonationHub.Domain.Campaigns;

public sealed class Campaign : AuditableEntity, ISoftDeletable
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
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    // Explicit interface implementation for ISoftDeletable
    bool ISoftDeletable.IsDeleted
    {
        get => IsDeleted;
        set => IsDeleted = value;
    }

    DateTime? ISoftDeletable.DeletedAtUtc
    {
        get => DeletedAtUtc;
        set => DeletedAtUtc = value;
    }

    private Campaign() { }

    // Rules 1, 2
    public static Campaign Create(
        string titleAr, string titleEn, string descriptionAr, string descriptionEn,
        decimal goalAmount, DateTime endDate, Guid categoryId, Guid createdByUserId, DateTime now)
    {
        EnsureGoalIsPositive(goalAmount);
        EnsureEndDateInFuture(endDate, now);

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            GoalAmount = goalAmount,
            RaisedAmount = 0m,
            EndDate = endDate,
            Status = CampaignStatus.Active,
            CreatedByUserId = createdByUserId
        };

        campaign.MarkCreated(now);
        campaign.ApplyDetails(titleAr, titleEn, descriptionAr, descriptionEn, categoryId, now);
        return campaign;
    }

    // Rules 1, 2, 7, 8 (CMP-02)
    public void Update(
        string titleAr, string titleEn, string descriptionAr, string descriptionEn,
        Guid categoryId, decimal goalAmount, DateTime endDate, DateTime now)
    {
        EnsureActive();                                   // Rule 8
        EnsureGoalIsPositive(goalAmount);                 // Rule 1
        EnsureGoalNotBelowRaised(goalAmount);             // Rule 7
        if (endDate != EndDate)
            EnsureEndDateInFuture(endDate, now);          // Rule 2

        ApplyDetails(titleAr, titleEn, descriptionAr, descriptionEn, categoryId, now);

        GoalAmount = goalAmount;
        EndDate = endDate;

        // Automatically mark as completed if the raised amount meets or exceeds the new goal
        if (RaisedAmount >= GoalAmount)
            Status = CampaignStatus.Completed;
    }

    // Rules 1, 7, 8
    public void ChangeGoal(decimal newGoal, DateTime now)
    {
        EnsureActive();
        EnsureGoalIsPositive(newGoal);
        EnsureGoalNotBelowRaised(newGoal);

        GoalAmount = newGoal;
        Touch(now);
    }

    // Rules 2, 8
    public void ChangeEndDate(DateTime newEndDate, DateTime now)
    {
        EnsureActive();
        EnsureEndDateInFuture(newEndDate, now);

        EndDate = newEndDate;
        Touch(now);
    }

    public void SetBanner(string? fileName, DateTime now)
    {
        BannerFileName = fileName;
        Touch(now);
    }

    // Rule 3
    public void EnsureCanAcceptDonation(DateTime now)
    {
        if (IsDeleted || Status != CampaignStatus.Active || EndDate <= now)
            throw new DomainException("Campaign.NotActive", "Campaign is not accepting donations.");
    }

    
    internal void RegisterSucceededDonation(decimal amount, DateTime now)
    {
        RaisedAmount += amount;
        Touch(now);

        if (Status == CampaignStatus.Active && RaisedAmount >= GoalAmount)
            Status = CampaignStatus.Completed;
    }

    // Rule 8
    public void MarkExpired(DateTime now)
    {
        EnsureActive();
        if (EndDate > now)
            throw new DomainException("campaign.not_ended", "Campaign has not ended yet.");

        Status = CampaignStatus.Expired;
        Touch(now);
    }

    // Rule 9
    public void SoftDelete(DateTime now)
    {
        if (IsDeleted)
            throw new DomainException("campaign.already_deleted", "Campaign is already deleted.");

        IsDeleted = true;
        DeletedAtUtc = now;
        Touch(now);
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
        Touch(now);
    }

    private void EnsureActive()
    {
        if (Status != CampaignStatus.Active)
            throw new DomainException("Campaign.NotActive",
                "A completed or expired campaign cannot change.");
    }

    private void EnsureGoalNotBelowRaised(decimal goal)
    {
        if (goal < RaisedAmount)
            throw new DomainException("Campaign.GoalBelowRaised",
                "Goal amount cannot be lower than the amount already raised.");
    }

    private static void EnsureGoalIsPositive(decimal goal)
    {
        if (goal <= 0)
            throw new DomainException("Campaign.InvalidGoal", "Goal amount must be greater than zero.");
    }

    private static void EnsureEndDateInFuture(DateTime endDate, DateTime now)
    {
        if (endDate <= now)
            throw new DomainException("Campaign.EndDateInPast", "End date must be in the future.");
    }
}