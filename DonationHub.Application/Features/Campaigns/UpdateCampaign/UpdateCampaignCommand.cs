using System.Text.Json.Serialization;
using DonationHub.Application.Common;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Categories;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.UpdateCampaign;

public sealed record UpdateCampaignCommand : IRequest<Result<CampaignAdminDto>>
{
    [JsonIgnore] public Guid Id { get; init; }   
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public Guid CategoryId { get; init; }
    public decimal GoalAmount { get; init; }
    public DateTime EndDate { get; init; }
    public byte[] RowVersion { get; init; } = [];   
}

public sealed class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DescriptionAr).MaximumLength(4000);
        RuleFor(x => x.DescriptionEn).MaximumLength(4000);
        RuleFor(x => x.GoalAmount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}

internal sealed class UpdateCampaignCommandHandler(
    ICampaignRepository campaigns,
    ICategoryRepository categories,
    IUnitOfWork uow,
    TimeProvider clock) : IRequestHandler<UpdateCampaignCommand, Result<CampaignAdminDto>>
{
    public async Task<Result<CampaignAdminDto>> Handle(UpdateCampaignCommand cmd, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var campaign = await campaigns.GetByIdAsync(cmd.Id, ct);   
        if (campaign is null) return CampaignErrors.NotFound();

        if (await categories.GetByIdAsync(cmd.CategoryId, ct) is null)
            return CategoryErrors.NotFound();

        campaigns.SetExpectedRowVersion(campaign, cmd.RowVersion);   // CMP-10

        campaign.Update(
            cmd.TitleAr, cmd.TitleEn, cmd.DescriptionAr ?? "", cmd.DescriptionEn ?? "",
            cmd.CategoryId, cmd.GoalAmount, cmd.EndDate.ToUtc(), now);

        await uow.SaveChangesAsync(ct);

        return CampaignAdminDto.From(campaign);
    }
}