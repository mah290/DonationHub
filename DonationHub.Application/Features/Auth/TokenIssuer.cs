using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public sealed class TokenIssuer(ITokenService tokens)
    {
        public (RefreshToken Entity, AuthResponse Response) Issue(UserInfo user, DateTime now)
        {
            var raw = tokens.GenerateRefreshToken();
            var entity = RefreshToken.Create(user.Id, tokens.Hash(raw), now, now.Add(tokens.RefreshTokenLifetime));
            var access = tokens.CreateAccessToken(user, now);

            return (entity, new AuthResponse(access.Token, access.ExpiresAtUtc, raw, entity.ExpiresAt));
        }
    }
}
