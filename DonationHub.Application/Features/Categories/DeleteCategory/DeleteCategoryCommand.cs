using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using MediatR;

namespace DonationHub.Application.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteCategoryCommandHandler(ICategoryRepository categories, IUnitOfWork uow)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.Id, ct);
        if (category is null) return CategoryErrors.NotFound();

        if (await categories.HasCampaignsAsync(cmd.Id, ct))
            return CategoryErrors.HasCampaigns();

        categories.Remove(category);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}