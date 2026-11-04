using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Application.Features.Categories
{
    public sealed record CategoryRequest
    {
        [Required, MaxLength(100)]
        public string NameAr { get; init; } = string.Empty;

        [Required, MaxLength(100)]
        public string NameEn { get; init; } = string.Empty;

        [MaxLength(1000)]
        public string? DescriptionAr { get; init; }

        [MaxLength(1000)]
        public string? DescriptionEn { get; init; }
    }

    public sealed record CategoryDto(
        Guid Id,
        string NameAr,
        string NameEn,
        string DescriptionAr,
        string DescriptionEn,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
