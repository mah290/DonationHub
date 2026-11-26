using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.CancelCampaign;

public sealed record CancelCampaignCommand(Guid Id) : IRequest<Result<CampaignAdminDto>>;

public sealed class CancelCampaignCommandValidator : AbstractValidator<CancelCampaignCommand>
{
    public CancelCampaignCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

internal sealed class CancelCampaignCommandHandler(
    ICampaignRepository campaigns,ICampaignAuthorizer authorizer, IUnitOfWork uow, TimeProvider clock)
    : IRequestHandler<CancelCampaignCommand, Result<CampaignAdminDto>>
{
    public async Task<Result<CampaignAdminDto>> Handle(CancelCampaignCommand cmd, CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdAsync(cmd.Id, ct);
        if (campaign is null || !await authorizer.CanManageAsync(campaign,ct)) return CampaignErrors.NotFound();

        campaign.Cancel(clock.GetUtcNow().UtcDateTime);   
        await uow.SaveChangesAsync(ct);

        return CampaignAdminDto.From(campaign);
    }
}