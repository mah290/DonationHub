using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        AccessToken CreateAccessToken(UserInfo user, DateTime now);
        string GenerateRefreshToken();
        string Hash(string token);
        TimeSpan RefreshTokenLifetime { get; }
    }
}
