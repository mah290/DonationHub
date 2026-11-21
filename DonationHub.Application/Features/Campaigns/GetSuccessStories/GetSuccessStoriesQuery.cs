using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Common.Specifications;
using DonationHub.Domain.Enums;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.GetSuccessStories;

public sealed record GetSuccessStoriesQuery(string Language)
    : IRequest<Result<IReadOnlyList<CampaignListItemDto>>>;

public sealed class GetSuccessStoriesQueryValidator : AbstractValidator<GetSuccessStoriesQuery>
{
    public GetSuccessStoriesQueryValidator() =>
        RuleFor(x => x.Language).Must(Languages.IsSupported).WithMessage("Language must be 'en' or 'ar'.");
}

internal sealed class GetSuccessStoriesQueryHandler(
    IReadDbContext read, IAsyncQueryExecutor query, TimeProvider clock)
    : IRequestHandler<GetSuccessStoriesQuery, Result<IReadOnlyList<CampaignListItemDto>>>
{
    public async Task<Result<IReadOnlyList<CampaignListItemDto>>> Handle(
        GetSuccessStoriesQuery q, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var rows = await query.ToListAsync(
            read.Campaigns
                .Satisfying(CampaignSpecifications.Completed())
                .OrderByDescending(c => c.UpdatedAt)
                .Select(CampaignReadModel.Projection(q.Language == Languages.Arabic)), ct);

        IReadOnlyList<CampaignListItemDto> items = rows.Select(r => CampaignReadModel.ToListItem(r, now)).ToList();
        return Result<IReadOnlyList<CampaignListItemDto>>.Success(items);
    }
}