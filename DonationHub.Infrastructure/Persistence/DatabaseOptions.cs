using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Infrastructure.Persistence
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "Database";

        [Required]
        public string ConnectionString { get; set; } = string.Empty;
    }
}
