using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DonationHub.Infrastructure.Identity;

internal sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(options.Value.RefreshTokenExpiryDays);

    public AccessToken CreateAccessToken(UserInfo user, DateTime now)
    {
        var jwt = options.Value;

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("jti", Guid.NewGuid().ToString())
        };
        claims.AddRange(user.Roles.Select(r => new Claim("role", r)));

        var expires = now.AddMinutes(jwt.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public string GenerateRefreshToken() =>
        Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}