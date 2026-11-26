using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DonationHub.Application.Features.Campaigns.GetMyCampaigns
{
    public sealed record GetMyCampaignsQuery : IRequest<Result<PagedResult<CampaignAdminDto>>>
    {
        [JsonIgnore] public Guid UserId { get; init; }   
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public sealed class GetMyCampaignsQueryValidator : AbstractValidator<GetMyCampaignsQuery>
    {
        public GetMyCampaignsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    internal sealed class GetMyCampaignsQueryHandler(IAsyncQueryExecutor query, IReadDbContext read) : IRequestHandler<GetMyCampaignsQuery, Result<PagedResult<CampaignAdminDto>>>
    {
        public async Task<Result<PagedResult<CampaignAdminDto>>> Handle(GetMyCampaignsQuery request, CancellationToken cancellationToken)
        {
            var mine = read.Campaigns.Where(c => c.CreatedByUserId == request.UserId);

            var TotalCount = await query.CountAsync(mine, cancellationToken);

            var items = await query.ToListAsync(mine.OrderByDescending(c => c.CreatedAt).Skip(request.Page - 1 * request.PageSize).Take(request.PageSize).Select(CampaignAdminDto.Projection), cancellationToken);

            return new PagedResult<CampaignAdminDto>(items, request.Page, request.PageSize, TotalCount);
        }
    }
}
