using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Common.Specifications;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.GetCampaigns;

public sealed record GetCampaignsQuery(string Language, Guid? CategoryId = null)
    : IRequest<Result<IReadOnlyList<CampaignListItemDto>>>;

public sealed class GetCampaignsQueryValidator : AbstractValidator<GetCampaignsQuery>
{
    public GetCampaignsQueryValidator() =>
        RuleFor(x => x.Language).Must(Languages.IsSupported).WithMessage("Language must be 'en' or 'ar'.");
}

internal sealed class GetCampaignsQueryHandler(
    IReadDbContext read, IAsyncQueryExecutor query, TimeProvider clock)
    : IRequestHandler<GetCampaignsQuery, Result<IReadOnlyList<CampaignListItemDto>>>
{
    public async Task<Result<IReadOnlyList<CampaignListItemDto>>> Handle(
        GetCampaignsQuery q, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var spec = CampaignSpecifications.Active();
        if (q.CategoryId is { } categoryId)
            spec = spec.And(CampaignSpecifications.InCategory(categoryId));

        var rows = await query.ToListAsync(
            read.Campaigns
                .Satisfying(spec)
                .OrderByDescending(c => c.CreatedAt)
                .Select(CampaignReadModel.Projection(q.Language == Languages.Arabic)), ct);

        IReadOnlyList<CampaignListItemDto> items = rows.Select(r => CampaignReadModel.ToListItem(r, now)).ToList();
        return Result<IReadOnlyList<CampaignListItemDto>>.Success(items);
    }
}