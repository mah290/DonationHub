using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth.Login
{
    public sealed record LoginCommand : IRequest<Result<AuthResponse>>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public sealed class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
        
    }

    public sealed class LoginCommandHandler(IIdentityService identity, TimeProvider time, TokenIssuer issuer
        , IUnitOfWork uow, IRefreshTokenRepository refreshToken) : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var auth = await identity.AuthenticateAsync(request.Email, request.Password, cancellationToken);
            if(auth.IsFailure) return auth.Error;

            var (entity,response) = issuer.Issue(auth.Value , time.GetUtcNow().UtcDateTime);

            await refreshToken.AddAsync(entity, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return response;

        }
    }

}
