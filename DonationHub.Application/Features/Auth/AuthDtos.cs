using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public sealed record RegisterRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; init; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;

        public string PreferredLanguage { get; init; } = "en";
    }

    public sealed record LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public sealed record RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }

    public sealed record AuthResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);

    public sealed record UserProfileDto(
        Guid Id,
        string Email,
        string FullName,
        string PreferredLanguage,
        IReadOnlyList<string> Roles,
        DateTime CreatedAt);
}
