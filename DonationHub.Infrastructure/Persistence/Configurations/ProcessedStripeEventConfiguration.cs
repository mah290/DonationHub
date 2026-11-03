using DonationHub.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class ProcessedStripeEventConfiguration : IEntityTypeConfiguration<ProcessedStripeEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
        {
            builder.ToTable("ProcessedStripeEvents");

            builder.HasKey(e => e.EventId);
            builder.Property(e => e.EventId).HasMaxLength(255).ValueGeneratedNever();

            builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
            builder.Property(e => e.ProcessedAt).IsRequired();
        }
    }
}
