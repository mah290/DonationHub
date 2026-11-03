using DonationHub.Application.Common.Exceptions;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Identity
{
    internal static class UserCreation
    {
        public static async Task<ApplicationUser> CreateAsync(
        UserManager<ApplicationUser> users,
        string email, string fullName, string password,
        string language, string role, DateTime now)
        {
            if (!Languages.IsSupported(language))
                throw new AppException("Auth.UnsupportedLanguage",
                    "Preferred language must be 'en' or 'ar'.", ErrorType.Validation);

            email = email.Trim();

            // FindByEmailAsync بيقارن بالـ NormalizedEmail (UPPER) = case-insensitive
            if (await users.FindByEmailAsync(email) is not null)
                throw AuthErrors.EmailAlreadyExists();

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
                // سباق: اتنين سجّلوا بنفس الإيميل في نفس اللحظة
                if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                    throw AuthErrors.EmailAlreadyExists();

                throw new AppException("Auth.ValidationFailed",
                    string.Join(" ", result.Errors.Select(e => e.Description)),
                    ErrorType.Validation);
            }

            var roleResult = await users.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await users.DeleteAsync(user);   // ميفضلش يوزر من غير Role
                throw new InvalidOperationException(
                    $"Failed to assign role '{role}': {string.Join(" ", roleResult.Errors.Select(e => e.Description))}");
            }

            return user;
        }
    }
}
