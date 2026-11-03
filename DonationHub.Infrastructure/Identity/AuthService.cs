using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Auth;
using DonationHub.Domain.Users;
using DonationHub.Infrastructure.Options;
using DonationHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Identity
{
    internal sealed class AuthService(
    UserManager<ApplicationUser> users,
    ITokenService tokens,
    AppDbContext db,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider clock) : IAuthService
    {
        private DateTime Now => clock.GetUtcNow().UtcDateTime;

        // IDN-01, IDN-02
        public async Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            var user = await UserCreation.CreateAsync(
                users, request.Email, request.FullName, request.Password,
                request.PreferredLanguage, Roles.Donor, Now);

            return await ToProfileAsync(user);
        }

        // IDN-03, IDN-04, IDN-10, IDN-12
        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var user = await users.FindByEmailAsync(request.Email.Trim());
            if (user is null || !await users.CheckPasswordAsync(user, request.Password))
                throw AuthErrors.InvalidCredentials();
            if (!user.IsActive)
                throw AuthErrors.AccountDeactivated();

            var now = Now;
            var (entity, raw) = CreateRefreshToken(user.Id, now);
            db.RefreshTokens.Add(entity);
            await db.SaveChangesAsync(ct);
            return await BuildResponseAsync(user, raw, entity, now);
        }

        // IDN-05, IDN-06, IDN-10
        public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct)
        {
            var now = Now;
            var hash = tokens.Hash(refreshToken);

            var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
            if (stored is null || !stored.IsActive(now))
                throw AuthErrors.RefreshTokenInvalid();

            var user = await users.FindByIdAsync(stored.UserId.ToString())
                       ?? throw AuthErrors.RefreshTokenInvalid();

            if (!user.IsActive)
                throw AuthErrors.AccountDeactivated();
            var (newEntity, raw) = CreateRefreshToken(user.Id, now);
            stored.Revoke(now, newEntity.Id);
            db.RefreshTokens.Add(newEntity);
            await db.SaveChangesAsync(ct);

            return await BuildResponseAsync(user, raw, newEntity, now);
        }

        // IDN-07
        public async Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct)
        {
            var hash = tokens.Hash(refreshToken);

            var stored = await db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == hash && t.UserId == userId, ct);
            if (stored is { IsRevoked: false })
            {
                stored.Revoke(Now);
                await db.SaveChangesAsync(ct);
            }
        }

        // IDN-09
        public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct)
        {
            var user = await users.FindByIdAsync(userId.ToString())
                       ?? throw AuthErrors.InvalidCredentials();

            if (!user.IsActive)
                throw AuthErrors.AccountDeactivated();

            return await ToProfileAsync(user);
        }

        private (RefreshToken Entity, string Raw) CreateRefreshToken(Guid userId, DateTime now)
        {
            var raw = tokens.GenerateRefreshToken();
            var entity = RefreshToken.Create(
                userId, tokens.Hash(raw), now, now.AddDays(jwtOptions.Value.RefreshTokenExpiryDays));
            return (entity, raw);
        }

        private async Task<AuthResponse> BuildResponseAsync(
            ApplicationUser user, string rawRefreshToken, RefreshToken entity, DateTime now)
        {
            var roles = await users.GetRolesAsync(user);
            var (access, accessExpires) = tokens.CreateAccessToken(user, roles, now);

            return new AuthResponse(access, accessExpires, rawRefreshToken, entity.ExpiresAt);
        }

        private async Task<UserProfileDto> ToProfileAsync(ApplicationUser user)
        {
            var roles = await users.GetRolesAsync(user);
            return new UserProfileDto(
                user.Id, user.Email!, user.FullName, user.PreferredLanguage, roles.ToList(), user.CreatedAt);
        }
    }
}
