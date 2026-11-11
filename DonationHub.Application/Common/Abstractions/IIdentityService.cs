using DonationHub.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public interface IIdentityService
    {
        Task<Result<UserInfo>> CreateUserAsync(
            string email, string fullName, string password, string language, string role, CancellationToken ct);
        Task<Result<UserInfo>> AuthenticateAsync(string email, string password, CancellationToken ct);

        Task<UserInfo?> GetByIdAsync(Guid id, CancellationToken ct);

        Task<Result<UserInfo>> UpdateProfileAsync(Guid id, string fullName, string? language, CancellationToken ct);

        Task<Result> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken ct);
    }
}
