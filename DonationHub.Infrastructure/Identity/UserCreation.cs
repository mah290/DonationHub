using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace DonationHub.Infrastructure.Identity;

internal static class UserCreation
{
    public static async Task<Result<ApplicationUser>> CreateAsync(
        UserManager<ApplicationUser> users,
        string email, string fullName, string password,
        string language, string role, DateTime now)
    {
        if (!Languages.IsSupported(language))
            return new Error("Auth.UnsupportedLanguage",
                "Preferred language must be 'en' or 'ar'.", ErrorType.Validation);

        email = email.Trim();

        if (await users.FindByEmailAsync(email) is not null)
            return AuthErrors.EmailAlreadyExists();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FullName = fullName.Trim(),
            PreferredLanguage = language,
            IsActive = true,
            CreatedAt = now
        };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                return AuthErrors.EmailAlreadyExists();

            return AuthErrors.ValidationFailed(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);   
            throw new InvalidOperationException(
                $"Failed to assign role '{role}': {string.Join(" ", roleResult.Errors.Select(e => e.Description))}");
        }

        return user;
    }

    // This method is used for seeding users during application startup. It throws an exception if user creation fails.
    public static async Task<ApplicationUser> CreateOrThrowAsync(
        UserManager<ApplicationUser> users,
        string email, string fullName, string password,
        string language, string role, DateTime now)
    {
        var result = await CreateAsync(users, email, fullName, password, language, role, now);

        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"Seeding user failed: {result.Error.Code} - {result.Error.Message}");
    }
}