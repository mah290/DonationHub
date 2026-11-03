using DonationHub.Application.Features.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Application.Features.Admin
{
    public sealed record CreateStaffRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; init; } = string.Empty;

        [Required]
        public string TemporaryPassword { get; init; } = string.Empty;

        [Required]
        public string Role { get; init; } = string.Empty;   // Admin | CampaignManager

        public string PreferredLanguage { get; init; } = "en";
    }

    public interface IStaffService
    {
        Task<UserProfileDto> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct);
    }
}
