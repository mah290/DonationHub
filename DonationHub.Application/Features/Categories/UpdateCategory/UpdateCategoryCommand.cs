using System.Text.Json.Serialization;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand : IRequest<Result<CategoryDto>>
{
    [JsonIgnore] public Guid Id { get; init; }   
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
}

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DescriptionAr).MaximumLength(1000);
        RuleFor(x => x.DescriptionEn).MaximumLength(1000);
    }
}

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categories, IUnitOfWork uow, TimeProvider clock)
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.Id, ct);
        if (category is null) return CategoryErrors.NotFound();

        var nameAr = cmd.NameAr.Trim();
        var nameEn = cmd.NameEn.Trim();

        if (await categories.NameExistsAsync(nameAr, nameEn, cmd.Id, ct))
            return CategoryErrors.NameAlreadyExists();

        category.Update(nameAr, nameEn, cmd.DescriptionAr ?? "", cmd.DescriptionEn ?? "",
            clock.GetUtcNow().UtcDateTime);

        await uow.SaveChangesAsync(ct);
        return CategoryDto.From(category);
    }
}
