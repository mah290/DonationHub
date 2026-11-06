using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Categories
{
    public sealed class Category : AuditableEntity
    {
        public string NameAr { get; private set; } = default!;
        public string NameEn { get; private set; } = default!;
        public string DescriptionAr { get; private set; } = default!;
        public string DescriptionEn { get; private set; } = default!;
        

        private Category() { }

        public static Category Create(
            string nameAr, string nameEn, string descriptionAr, string descriptionEn, DateTime now)
        {
            var category = new Category { Id = Guid.NewGuid()};
            category.MarkCreated(now);
            category.Apply(nameAr, nameEn, descriptionAr, descriptionEn, now);
            return category;
        }

        public void Update(
            string nameAr, string nameEn, string descriptionAr, string descriptionEn, DateTime now)
            => Apply(nameAr, nameEn, descriptionAr, descriptionEn, now);

        // Invariant #10: a category that still has campaigns cannot be deleted.
        // The handler asks the DB and passes the answer in.
        public void EnsureCanBeDeleted(bool hasCampaigns)
        {
            if (hasCampaigns)
                throw new DomainException("category.has_campaigns",
                    "Cannot delete a category that still has campaigns.");
        }

        private void Apply(string nameAr, string nameEn, string descriptionAr, string descriptionEn, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(nameAr) || string.IsNullOrWhiteSpace(nameEn))
                throw new DomainException("category.name_required", "Arabic and English names are required.");
            if (nameAr.Trim().Length > 100 || nameEn.Trim().Length > 100)
                throw new DomainException("category.name_too_long", "Category names cannot exceed 100 characters.");

            NameAr = nameAr.Trim();
            NameEn = nameEn.Trim();
            DescriptionAr = descriptionAr?.Trim() ?? string.Empty;
            DescriptionEn = descriptionEn?.Trim() ?? string.Empty;
            Touch(now);
        }
    }
}
