using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Users;
using DonationHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DonationHub.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> users, TimeProvider clock) : IIdentityService
{
    public async Task<Result<UserInfo>> CreateUserAsync(
        string email, string fullName, string password, string language, string role, CancellationToken ct)
    {
        var created = await UserCreation.CreateAsync(
            users, email, fullName, password, language, role, clock.GetUtcNow().UtcDateTime);

        if (created.IsFailure) return created.Error;

        return await ToInfoAsync(created.Value);
    }

    public async Task<Result<UserInfo>> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email.Trim());

        if (user is null || !await users.CheckPasswordAsync(user, password))
            return AuthErrors.InvalidCredentials();

        if (!user.IsActive)
            return AuthErrors.AccountDeactivated();

        return await ToInfoAsync(user);
    }

    public async Task<UserInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id.ToString());
        return user is null ? null : await ToInfoAsync(user);
    }

    public async Task<Result<UserInfo>> UpdateProfileAsync(
        Guid id, string fullName, string? language, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return AuthErrors.InvalidCredentials();
        if (!user.IsActive) return AuthErrors.AccountDeactivated();

        user.FullName = fullName.Trim();
        user.PreferredLanguage = language ?? user.PreferredLanguage;

        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
            return AuthErrors.ValidationFailed(Describe(result));

        return await ToInfoAsync(user);
    }

    public async Task<Result> ChangePasswordAsync(
        Guid id, string currentPassword, string newPassword, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return AuthErrors.InvalidCredentials();
        if (!user.IsActive) return AuthErrors.AccountDeactivated();

        if (!await users.CheckPasswordAsync(user, currentPassword))
            return AuthErrors.CurrentPasswordIncorrect();

        var result = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            return AuthErrors.ValidationFailed(Describe(result));

        return Result.Success();
    }

    private async Task<UserInfo> ToInfoAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new UserInfo(
            user.Id, user.Email!, user.FullName, user.PreferredLanguage,
            user.IsActive, user.CreatedAt, roles.ToList());
    }

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
