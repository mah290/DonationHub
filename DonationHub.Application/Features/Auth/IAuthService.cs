using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public interface IAuthService
    {
        Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken ct);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
        Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct);
        Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct);
        Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
    }
}
