using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Categories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DonationHub.Application.Features.Categories
{
    public sealed class CategoryService(
        ICategoryRepository categoryRepository,
        IAsyncQueryExecutor query, IReadDbContext read , TimeProvider clock , IUnitOfWork uow)
    {
        private static readonly Expression<Func<Category, CategoryDto>> Projection = c => new CategoryDto(c.Id, c.NameAr, c.NameEn , c.DescriptionAr, c.DescriptionEn , c.CreatedAt , c.UpdatedAt);
        private static readonly Func<Category, CategoryDto> map = Projection.Compile();

        private DateTime Now => clock.GetUtcNow().UtcDateTime;

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await query.ToListAsync(read.Categories.OrderBy(c => c.NameEn).Select(Projection));
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await query.FirstOrDefaultAsync(read.Categories.Where(c => c.Id == id).Select(Projection)) ?? throw CategoryErrors.NotFound();
        }

        public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default)
        {
            var nameAr = request.NameAr.Trim();
            var nameEn = request.NameEn.Trim();

            if(await categoryRepository.NameExistsAsync(nameAr, nameEn, null, ct))
                throw CategoryErrors.NameAlreadyExists();
            var category = Category.Create(nameAr, nameEn, request.DescriptionAr??"", request.DescriptionEn??"", Now);

            await categoryRepository.AddAsync(category, ct);
            await uow.SaveChangesAsync(ct);
            return map(category);
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default)
        {
           var category = await categoryRepository.GetByIdAsync(id, ct) ?? throw CategoryErrors.NotFound();
            var nameAr = request.NameAr.Trim();
            var nameEn = request.NameEn.Trim();
            if (await categoryRepository.NameExistsAsync(nameAr, nameEn, id, ct))
                throw CategoryErrors.NameAlreadyExists();
            category.Update(nameAr, nameEn, request.DescriptionAr ?? "", request.DescriptionEn ?? "", Now);
            await uow.SaveChangesAsync(ct);
            return map(category);
        }   
        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var category = await categoryRepository.GetByIdAsync(id, ct) ?? throw CategoryErrors.NotFound();
            if(await categoryRepository.HasCampaignsAsync(id, ct))
                throw CategoryErrors.HasCampaigns();
            
            categoryRepository.Remove(category);
            await uow.SaveChangesAsync(ct);


        }

    }
}
