using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using MediatR;

namespace DonationHub.Application.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;

internal sealed class GetCategoryByIdQueryHandler(IReadDbContext read, IAsyncQueryExecutor query)
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery q, CancellationToken ct)
    {
        var dto = await query.FirstOrDefaultAsync(
            read.Categories.Where(c => c.Id == q.Id).Select(CategoryDto.Projection), ct);

        if (dto is null) return CategoryErrors.NotFound();

        return dto;
    }
}