using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Auth;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth.RefreshTokens
{
    public sealed record RefreshTokenCommand : IRequest<Result<AuthResponse>>
    {
        public string RefreshToken { get; init; } = string.Empty;
    }

    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
    }

    internal sealed class RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IIdentityService identity,
        ITokenService tokens,
        TokenIssuer issuer,
        IUnitOfWork uow,
        TimeProvider clock) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
    {
        public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
        {
            var now = clock.GetUtcNow().UtcDateTime;

            var stored = await refreshTokens.GetByHashAsync(tokens.Hash(cmd.RefreshToken), ct);
            if (stored is null) return AuthErrors.RefreshTokenInvalid();

            if (stored.IsRevoked)
            {
                
                if (stored.ReplacedByTokenId is not null)
                    await RevokeFamilyAsync(stored, now, ct);

                return AuthErrors.RefreshTokenInvalid();
            }

            if (stored.IsExpired(now)) return AuthErrors.RefreshTokenInvalid();

            var user = await identity.GetByIdAsync(stored.UserId, ct);
            if (user is null) return AuthErrors.RefreshTokenInvalid();
            if (!user.IsActive) return AuthErrors.AccountDeactivated();

            // Rotation
            var (newEntity, response) = issuer.Issue(user, now);
            stored.Revoke(now, newEntity.Id);
            await refreshTokens.AddAsync(newEntity, ct);
            await uow.SaveChangesAsync(ct);

            return response;
        }

        private async Task RevokeFamilyAsync(RefreshToken reused, DateTime now, CancellationToken ct)
        {
            var byId = (await refreshTokens.GetByUserAsync(reused.UserId, ct)).ToDictionary(t => t.Id);

            var current = reused;
            while (current.ReplacedByTokenId is { } nextId && byId.TryGetValue(nextId, out var next))
            {
                if (!next.IsRevoked) next.Revoke(now);
                current = next;
            }

            await uow.SaveChangesAsync(ct);
        }
    }
}
