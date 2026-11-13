using DonationHub.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            builder.Property(u => u.PreferredLanguage).IsRequired().HasMaxLength(2);
            builder.Property(u => u.IsActive).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();
        }
    }
}
