using DonationHub.Application.Common.Exceptions;
using DonationHub.Application.Features.Admin;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Identity
{
    internal sealed class StaffService(
    UserManager<ApplicationUser> users,
    TimeProvider clock) : IStaffService
    {
        public async Task<UserProfileDto> CreateStaffAsync(CreateStaffRequest request, CancellationToken ct)
        {
            if (request.Role is not (Roles.Admin or Roles.CampaignManager))
                throw new AppException("Admin.InvalidStaffRole",
                    "Role must be 'Admin' or 'CampaignManager'.", ErrorType.Validation);

            var user = await UserCreation.CreateAsync(
                users, request.Email, request.FullName, request.TemporaryPassword,
                request.PreferredLanguage, request.Role, clock.GetUtcNow().UtcDateTime);

            return new UserProfileDto(
                user.Id, user.Email!, user.FullName, user.PreferredLanguage,
                [request.Role], user.CreatedAt);
        }
    }
}
