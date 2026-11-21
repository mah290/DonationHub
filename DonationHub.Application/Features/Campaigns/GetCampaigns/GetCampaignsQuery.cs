using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.GetCampaigns;

public sealed record GetCampaignsQuery(string Language) : IRequest<Result<IReadOnlyList<CampaignListItemDto>>>;

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

        var rows = await query.ToListAsync(
            read.Campaigns
                .Where(c => c.Status == CampaignStatus.Active)
                .OrderByDescending(c => c.CreatedAt)
                .Select(CampaignReadModel.Projection(q.Language == Languages.Arabic)), ct);

        IReadOnlyList<CampaignListItemDto> items = rows.Select(r => CampaignReadModel.ToListItem(r, now)).ToList();
        return Result<IReadOnlyList<CampaignListItemDto>>.Success(items);
    }
}