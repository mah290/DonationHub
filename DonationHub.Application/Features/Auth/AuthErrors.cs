using DonationHub.Application.Common.Results;
using DonationHub.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public static class AuthErrors
    {
        public static Error InvalidCredentials() =>
            new("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized);

        public static Error EmailAlreadyExists() =>
            new("Auth.EmailAlreadyExists", "Email is already registered.", ErrorType.Conflict);

        public static Error RefreshTokenInvalid() =>
            new("Auth.RefreshTokenInvalid", "Refresh token is invalid, expired or revoked.", ErrorType.Unauthorized);

        public static Error AccountDeactivated() =>
            new("Auth.AccountDeactivated", "This account is deactivated.", ErrorType.Forbidden);

        public static Error CurrentPasswordIncorrect() =>
            new("Auth.CurrentPasswordIncorrect", "Current password is incorrect.", ErrorType.Validation);

        public static Error ValidationFailed(string message) =>
            new("Auth.ValidationFailed", message, ErrorType.Validation);
    }
}
