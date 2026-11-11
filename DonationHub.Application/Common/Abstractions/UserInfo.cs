using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Common.Abstractions
{
    public sealed record UserInfo(
        Guid Id,
    string Email,
    string FullName,
    string PreferredLanguage,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles);

}
