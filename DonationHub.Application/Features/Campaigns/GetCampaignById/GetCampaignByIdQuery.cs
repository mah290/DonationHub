using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid Id, string Language) : IRequest<Result<CampaignDetailsDto>>;

public sealed class GetCampaignByIdQueryValidator : AbstractValidator<GetCampaignByIdQuery>
{
    public GetCampaignByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Language).Must(Languages.IsSupported).WithMessage("Language must be 'en' or 'ar'.");
    }
}

internal sealed class GetCampaignByIdQueryHandler(
    IReadDbContext read, IAsyncQueryExecutor query, TimeProvider clock)
    : IRequestHandler<GetCampaignByIdQuery, Result<CampaignDetailsDto>>
{
    public async Task<Result<CampaignDetailsDto>> Handle(GetCampaignByIdQuery q, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var row = await query.FirstOrDefaultAsync(
            read.Campaigns
                .Where(c => c.Id == q.Id)
                .Select(CampaignReadModel.Projection(q.Language == Languages.Arabic)), ct);

        if (row is null) return CampaignErrors.NotFound();

        return CampaignReadModel.ToDetails(row, now);
    }
}