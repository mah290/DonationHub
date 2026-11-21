using System.Text.Json.Serialization;
using DonationHub.Application.Common;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Campaigns;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Campaigns.CreateCampaign;

public sealed record CreateCampaignCommand : IRequest<Result<CampaignAdminDto>>
{
    [JsonIgnore] public Guid UserId { get; init; }   // من الـ token
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public decimal GoalAmount { get; init; }
    public DateTime EndDate { get; init; }
    public Guid CategoryId { get; init; }
}

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DescriptionAr).MaximumLength(4000);
        RuleFor(x => x.DescriptionEn).MaximumLength(4000);
        RuleFor(x => x.GoalAmount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.EndDate).NotEmpty();   // "في الماضي" قاعدة Domain مش Validator
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

internal sealed class CreateCampaignCommandHandler(
    ICampaignRepository campaigns,
    ICategoryRepository categories,
    IUnitOfWork uow,
    TimeProvider clock) : IRequestHandler<CreateCampaignCommand, Result<CampaignAdminDto>>
{
    public async Task<Result<CampaignAdminDto>> Handle(CreateCampaignCommand cmd, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        if (await categories.GetByIdAsync(cmd.CategoryId, ct) is null)
            return CategoryErrors.NotFound();

        var campaign = Campaign.Create(
            cmd.TitleAr, cmd.TitleEn, cmd.DescriptionAr ?? "", cmd.DescriptionEn ?? "",
            cmd.GoalAmount, cmd.EndDate.ToUtc(), cmd.CategoryId, cmd.UserId, now);

        await campaigns.AddAsync(campaign, ct);
        await uow.SaveChangesAsync(ct);

        return CampaignAdminDto.From(campaign);
    }
}