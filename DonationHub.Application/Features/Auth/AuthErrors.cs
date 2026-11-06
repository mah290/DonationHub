using DonationHub.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth
{
    public static class AuthErrors
    {
        public static AppException InvalidCredentials() =>
        new("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized);

        public static AppException EmailAlreadyExists() =>
            new("Auth.EmailAlreadyExists", "Email is already registered.", ErrorType.Conflict);

        public static AppException RefreshTokenInvalid() =>
            new("Auth.RefreshTokenInvalid", "Refresh token is invalid, expired or revoked.", ErrorType.Unauthorized);

        public static AppException AccountDeactivated() =>
            new("Auth.AccountDeactivated", "This account is deactivated.", ErrorType.Forbidden);
        public static AppException CurrentPasswordInCorrect()=>
            new("Auth.CurrentPasswordInCorrect", "Current password is incorrect.", ErrorType.Validation);
    }
}
