using DonationHub.Application.Common.Specifications;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Enums;

namespace DonationHub.Application.Features.Campaigns;

public static class CampaignSpecifications
{
    public static Specification<Campaign> Active() =>
        new ExpressionSpecification<Campaign>(c => c.Status == CampaignStatus.Active);

    public static Specification<Campaign> Completed() =>
        new ExpressionSpecification<Campaign>(c => c.Status == CampaignStatus.Completed);

    public static Specification<Campaign> InCategory(Guid categoryId) =>
        new ExpressionSpecification<Campaign>(c => c.CategoryId == categoryId);

    public static Specification<Campaign> EndingBefore(DateTime date) =>
        new ExpressionSpecification<Campaign>(c => c.EndDate < date);

    public static Specification<Campaign> TitleContains(string term) =>
        new ExpressionSpecification<Campaign>(c => c.TitleAr.Contains(term) || c.TitleEn.Contains(term));
}