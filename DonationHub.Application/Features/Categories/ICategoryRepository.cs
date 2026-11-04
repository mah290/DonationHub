using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Categories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> HasCampaignsAsync(Guid categoryId, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string nameAr, string nameEn, Guid? excludeId, CancellationToken ct = default);
    }
}
