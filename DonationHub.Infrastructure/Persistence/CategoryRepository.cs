using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    internal sealed class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context), ICategoryRepository
    {

        public Task<bool> HasCampaignsAsync(Guid categoryId, CancellationToken ct = default)
        {
            return Context.Campaigns.IgnoreQueryFilters().AnyAsync(c => c.CategoryId == categoryId, ct);
                
        }

        public Task<bool> NameExistsAsync(string nameAr, string nameEn, Guid? excludeId, CancellationToken ct = default)
        {
            return Context.Categories.AnyAsync(c=>(c.NameAr == nameAr || c.NameEn==nameEn) && excludeId == null || c.Id != excludeId, ct);
        }

    }
}
