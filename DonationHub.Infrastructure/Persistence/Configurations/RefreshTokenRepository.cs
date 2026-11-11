using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace DonationHub.Infrastructure.Persistence;

internal sealed class RefreshTokenRepository(AppDbContext context)
    : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        Context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(
        Guid userId, DateTime now, CancellationToken ct = default) =>
        await Context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);
}