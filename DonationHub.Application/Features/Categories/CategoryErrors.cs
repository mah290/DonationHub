using DonationHub.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Categories
{
    public static class CategoryErrors
    {
        public static AppException NotFound() =>
            new("Category.NotFound", "Category was not found.", ErrorType.NotFound);

        public static AppException HasCampaigns() =>
            new("Category.HasCampaigns", "Cannot delete a category that still has campaigns.", ErrorType.Conflict);

        public static AppException NameAlreadyExists() =>
            new("Category.NameAlreadyExists",
                "A category with the same Arabic or English name already exists.", ErrorType.Conflict);
    }
}
