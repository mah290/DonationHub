using DonationHub.Domain.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(i => new { i.Key, i.UserId });

            builder.Property(i => i.Key).HasMaxLength(128);
            builder.Property(i => i.RequestHash).IsRequired().HasMaxLength(64);
            builder.Property(i => i.ResponseStatusCode).IsRequired();
            builder.Property(i => i.ResponseBody).IsRequired();   
            builder.Property(i => i.CreatedAt).IsRequired();
            builder.Property(i => i.ExpiresAt).IsRequired();

            builder.HasIndex(i => i.ExpiresAt); 
        }
    }
}
