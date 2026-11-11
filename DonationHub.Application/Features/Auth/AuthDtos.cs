using DonationHub.Application.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
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
        DateTime CreatedAt)
    {
        public static UserProfileDto From(UserInfo u) =>
            new(u.Id, u.Email, u.FullName, u.PreferredLanguage, u.Roles, u.CreatedAt);
    }
}
