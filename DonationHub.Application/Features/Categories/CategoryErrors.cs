using DonationHub.Application.Common.Results;

namespace DonationHub.Application.Features.Categories;

public static class CategoryErrors
{
    public static Error NotFound() =>
        new("Category.NotFound", "Category was not found.", ErrorType.NotFound);

    public static Error HasCampaigns() =>
        new("Category.HasCampaigns", "Cannot delete a category that still has campaigns.", ErrorType.Conflict);

    public static Error NameAlreadyExists() =>
        new("Category.NameAlreadyExists",
            "A category with the same Arabic or English name already exists.", ErrorType.Conflict);
}
