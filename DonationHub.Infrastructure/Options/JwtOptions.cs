using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Infrastructure.Options
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required, MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters.")]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpiryMinutes { get; set; }

        [Range(1, 90)]
        public int RefreshTokenExpiryDays { get; set; }
    }
}
