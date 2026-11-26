using System.Linq.Expressions;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Enums;

namespace DonationHub.Application.Features.Campaigns;

public sealed record CampaignAdminDto(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string DescriptionAr,
    string DescriptionEn,
    decimal GoalAmount,
    decimal RaisedAmount,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    CampaignStatus Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    byte[] RowVersion)
{
    public static CampaignAdminDto From(Campaign c) => new(
        c.Id, c.TitleAr, c.TitleEn, c.DescriptionAr, c.DescriptionEn, c.GoalAmount, c.RaisedAmount,
        c.EndDate, c.CategoryId, c.BannerFileName, c.Status, c.CreatedByUserId,
        c.CreatedAt, c.UpdatedAt, c.RowVersion);

    public static readonly Expression<Func<Campaign, CampaignAdminDto>> Projection = c => new CampaignAdminDto(
    c.Id, c.TitleAr, c.TitleEn, c.DescriptionAr, c.DescriptionEn, c.GoalAmount, c.RaisedAmount,
    c.EndDate, c.CategoryId, c.BannerFileName, c.Status, c.CreatedByUserId,
    c.CreatedAt, c.UpdatedAt, c.RowVersion);
}

public sealed record CampaignListItemDto(
    Guid Id,
    string Title,
    string Description,
    decimal GoalAmount,
    decimal RaisedAmount,
    decimal ProgressPercent,
    int DaysLeft,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    CampaignStatus Status);

public sealed record CampaignDetailsDto(
    Guid Id,
    string Title,
    string Description,
    decimal GoalAmount,
    decimal RaisedAmount,
    decimal ProgressPercent,
    int DaysLeft,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    CampaignStatus Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    byte[] RowVersion);

internal sealed record CampaignRow(
    Guid Id,
    string Title,
    string Description,
    decimal GoalAmount,
    decimal RaisedAmount,
    DateTime EndDate,
    Guid CategoryId,
    string? BannerFileName,
    CampaignStatus Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    byte[] RowVersion);

internal static class CampaignReadModel
{
    public static Expression<Func<Campaign, CampaignRow>> Projection(bool arabic) =>
        c => new CampaignRow(
            c.Id,
            arabic ? c.TitleAr : c.TitleEn,
            arabic ? c.DescriptionAr : c.DescriptionEn,
            c.GoalAmount, c.RaisedAmount, c.EndDate, c.CategoryId, c.BannerFileName,
            c.Status, c.CreatedByUserId, c.CreatedAt, c.UpdatedAt, c.RowVersion);

    public static decimal Progress(decimal raised, decimal goal) =>
        goal <= 0 ? 0m : Math.Min(100m, Math.Round(raised / goal * 100m, 2));
    public static int DaysLeft(DateTime endDate, DateTime now) =>
        Math.Max(0, (int)Math.Ceiling((endDate - now).TotalDays));

    public static CampaignListItemDto ToListItem(CampaignRow r, DateTime now) => new(
        r.Id, r.Title, r.Description, r.GoalAmount, r.RaisedAmount,
        Progress(r.RaisedAmount, r.GoalAmount), DaysLeft(r.EndDate, now),
        r.EndDate, r.CategoryId, r.BannerFileName, r.Status);

    public static CampaignDetailsDto ToDetails(CampaignRow r, DateTime now) => new(
        r.Id, r.Title, r.Description, r.GoalAmount, r.RaisedAmount,
        Progress(r.RaisedAmount, r.GoalAmount), DaysLeft(r.EndDate, now),
        r.EndDate, r.CategoryId, r.BannerFileName, r.Status, r.CreatedByUserId,
        r.CreatedAt, r.UpdatedAt, r.RowVersion);
}