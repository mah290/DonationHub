using System.Text.Json.Serialization;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Auth.Logout;

public sealed record LogoutCommand : IRequest<Result>
{
    [JsonIgnore] public Guid UserId { get; init; }   
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

internal sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ITokenService tokens,
    IUnitOfWork uow,
    TimeProvider clock) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var stored = await refreshTokens.GetByHashAsync(tokens.Hash(cmd.RefreshToken), ct);

        if (stored is { IsRevoked: false } && stored.UserId == cmd.UserId)
        {
            stored.Revoke(clock.GetUtcNow().UtcDateTime);
            await uow.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}