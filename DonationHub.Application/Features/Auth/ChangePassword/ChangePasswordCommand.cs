using System.Text.Json.Serialization;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Auth.ChangePassword;

public sealed record ChangePasswordCommand : IRequest<Result>
{
    [JsonIgnore] public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty();
    }
}

internal sealed class ChangePasswordCommandHandler(
    IIdentityService identity,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    TimeProvider clock) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        var result = await identity.ChangePasswordAsync(cmd.UserId, cmd.CurrentPassword, cmd.NewPassword, ct);
        if (result.IsFailure) return result;

        var now = clock.GetUtcNow().UtcDateTime;
        foreach (var token in await refreshTokens.GetActiveByUserAsync(cmd.UserId, now, ct))
            token.Revoke(now);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
