using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DonationHub.Domain.Categories;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property(c => c.NameAr).IsRequired().HasMaxLength(100);
            builder.Property(c => c.NameEn).IsRequired().HasMaxLength(100);
            builder.Property(c => c.DescriptionAr).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.DescriptionEn).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UpdatedAt).IsRequired();

            builder.HasIndex(c => c.NameAr).IsUnique();
            builder.HasIndex(c => c.NameEn).IsUnique();
        }
    }
}
