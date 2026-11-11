using DonationHub.Application.Common.Abstractions;
using DonationHub.Domain.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshToken>> GetByUserAsync(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(Guid userId, DateTime now, CancellationToken ct = default);
    }
}
