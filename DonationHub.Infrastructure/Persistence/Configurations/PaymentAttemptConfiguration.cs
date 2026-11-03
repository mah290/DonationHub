using DonationHub.Domain.Donations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.ToTable("PaymentAttempts");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.StripeCheckoutSessionId).IsRequired().HasMaxLength(255);
            builder.Property(p => p.StripePaymentIntentId).HasMaxLength(255);
            builder.Property(p => p.Amount).HasPrecision(18, 2);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt).IsRequired();
            builder.Property(p => p.FailureCode).HasMaxLength(100);

            builder.HasOne<Donation>()
                .WithMany()
                .HasForeignKey(p => p.DonationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.DonationId);
            builder.HasIndex(p => p.StripeCheckoutSessionId).IsUnique();
            builder.HasIndex(p => p.StripePaymentIntentId);
        }
    }
}
