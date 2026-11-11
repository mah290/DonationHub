using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using MediatR;

namespace DonationHub.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

internal sealed class GetCategoriesQueryHandler(IReadDbContext read, IAsyncQueryExecutor query)
    : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery q, CancellationToken ct)
    {
        var items = await query.ToListAsync(
            read.Categories.OrderBy(c => c.NameEn).Select(CategoryDto.Projection), ct);

        return Result<IReadOnlyList<CategoryDto>>.Success(items);
    }
}