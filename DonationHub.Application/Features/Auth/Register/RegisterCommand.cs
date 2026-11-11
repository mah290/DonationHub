using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Auth.Register
{
    public sealed record RegisterCommand : IRequest<Result<UserProfileDto>>
    {
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string PreferredLanguage { get; init; } = Languages.English;
    }

    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x =>x.
            Email)
                .NotEmpty()
                .EmailAddress().MaximumLength(256);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.PreferredLanguage).Must(Languages.IsSupported).WithMessage($"Preferred language must be one of the supported languages 'en' or 'ar' ");
        }
    }

    internal sealed class RegisterCommandHandler(IIdentityService identity) : IRequestHandler<RegisterCommand, Result<UserProfileDto>>
    {
        public async Task<Result<UserProfileDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var result = await identity.CreateUserAsync(request.Email, request.FullName, request.Password, request.PreferredLanguage, Roles.Donor, cancellationToken);
            if (result.IsFailure) return result.Error;
            return UserProfileDto.From(result.Value);
        }
    }
}
