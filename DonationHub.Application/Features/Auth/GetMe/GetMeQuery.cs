using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using MediatR;

namespace DonationHub.Application.Features.Auth.GetMe;

public sealed record GetMeQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

internal sealed class GetMeQueryHandler(IIdentityService identity)
    : IRequestHandler<GetMeQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetMeQuery query, CancellationToken ct)
    {
        var user = await identity.GetByIdAsync(query.UserId, ct);

        if (user is null) return AuthErrors.InvalidCredentials();
        if (!user.IsActive) return AuthErrors.AccountDeactivated();

        return UserProfileDto.From(user);
    }
}
