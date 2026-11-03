using DonationHub.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DonationHub.Infrastructure.Identity
{
    internal interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, DateTime now);
        string GenerateRefreshToken();
        string Hash(string token);
    }

    internal sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
    {
        public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(
            ApplicationUser user, IEnumerable<string> roles, DateTime now)
        {
            var jwt = options.Value;

            var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email!),
            new("jti", Guid.NewGuid().ToString())
        };
            claims.AddRange(roles.Select(r => new Claim("role", r)));

            var expires = now.AddMinutes(jwt.ExpiryMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        // 64 byte عشوائي = توكن مستحيل يتخمّن
        public string GenerateRefreshToken() =>
            Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

        // الـ DB بتخزّن الـ hash بس، مش التوكن نفسه
        public string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
