using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.DeleteCampaign;

public sealed record DeleteCampaignCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteCampaignCommandValidator : AbstractValidator<DeleteCampaignCommand>
{
    public DeleteCampaignCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

internal sealed class DeleteCampaignCommandHandler(
    ICampaignRepository campaigns, ICampaignAuthorizer authorizer, IUnitOfWork uow, TimeProvider clock)
    : IRequestHandler<DeleteCampaignCommand, Result>
{
    public async Task<Result> Handle(DeleteCampaignCommand cmd, CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdAsync(cmd.Id, ct);
        if (campaign is null || !await authorizer.CanManageAsync(campaign,ct)) return CampaignErrors.NotFound();

        campaign.SoftDelete(clock.GetUtcNow().UtcDateTime);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}