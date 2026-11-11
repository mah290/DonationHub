using System.Linq.Expressions;
using DonationHub.Domain.Categories;

namespace DonationHub.Application.Features.Categories;

public sealed record CategoryDto(
    Guid Id,
    string NameAr,
    string NameEn,
    string DescriptionAr,
    string DescriptionEn,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static readonly Expression<Func<Category, CategoryDto>> Projection = c => new CategoryDto(
        c.Id, c.NameAr, c.NameEn, c.DescriptionAr, c.DescriptionEn, c.CreatedAt, c.UpdatedAt);

    private static readonly Func<Category, CategoryDto> Compiled = Projection.Compile();
    public static CategoryDto From(Category category) => Compiled(category);
}
