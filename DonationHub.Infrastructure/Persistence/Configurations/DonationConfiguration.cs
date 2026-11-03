using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Donations;
using DonationHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class DonationConfiguration : IEntityTypeConfiguration<Donation>
    {
        public void Configure(EntityTypeBuilder<Donation> builder)
        {
            builder.ToTable("Donations");

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedNever();

            builder.Property(d => d.Amount).HasPrecision(18, 2);

            builder.Property(d => d.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(d => d.CreatedAt).IsRequired();
            builder.Property(d => d.FailureReason).HasMaxLength(500);
            builder.Property(d => d.ReceiptNumber).HasMaxLength(50);
            builder.Property(d => d.ReceiptFileName).HasMaxLength(260);

            // Campaign -> Donations (Restrict)
            builder.HasOne<Campaign>()
                .WithMany()
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Donations
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.DonorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(d => d.CampaignId);
            builder.HasIndex(d => d.DonorUserId);
            builder.HasIndex(d => d.ReceiptNumber).IsUnique();
        }
    }
}
