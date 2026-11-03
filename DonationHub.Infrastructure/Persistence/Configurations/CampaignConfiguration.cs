using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Categories;
using DonationHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaigns");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property(c => c.TitleAr).IsRequired().HasMaxLength(200);
            builder.Property(c => c.TitleEn).IsRequired().HasMaxLength(200);
            builder.Property(c => c.DescriptionAr).IsRequired().HasMaxLength(4000);
            builder.Property(c => c.DescriptionEn).IsRequired().HasMaxLength(4000);

            builder.Property(c => c.GoalAmount).HasPrecision(18, 2);
            builder.Property(c => c.RaisedAmount).HasPrecision(18, 2);

            builder.Property(c => c.EndDate).IsRequired();
            builder.Property(c => c.BannerFileName).HasMaxLength(260);

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UpdatedAt).IsRequired();
            builder.Property(c => c.IsDeleted).IsRequired();
            builder.Property(c => c.DeletedAtUtc); //Added to support soft delete in task 5

            // Optimistic concurrency
            builder.Property(c => c.RowVersion).IsRowVersion();
            builder.HasQueryFilter(c => !c.IsDeleted); //Added in task 5 to support soft delete

            // Category -> Campaigns (Restrict)
            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Campaigns
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.CategoryId);
            builder.HasIndex(c => c.CreatedByUserId);
            builder.HasIndex(c => new { c.Status, c.EndDate });
        }
    }
}
