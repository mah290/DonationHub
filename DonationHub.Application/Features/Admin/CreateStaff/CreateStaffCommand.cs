using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace DonationHub.Application.Features.Admin.CreateStaff
{
    public sealed record CreateStaffCommand : IRequest<Result<UserProfileDto>>
    {
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string TemporaryPassword { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;   // Admin | CampaignManager
        public string PreferredLanguage { get; init; } = Languages.English;
    }   

    public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
    {
        public CreateStaffCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.TemporaryPassword).NotEmpty();
            RuleFor(x => x.Role)
                .Must(r => r is Roles.Admin or Roles.CampaignManager)
                .WithMessage("Role must be 'Admin' or 'CampaignManager'.");
            RuleFor(x => x.PreferredLanguage)
                .Must(Languages.IsSupported).WithMessage("Preferred language must be 'en' or 'ar'.");
        }
    }

    public sealed class CreateStaffCommandHandler(IIdentityService identity) : IRequestHandler<CreateStaffCommand, Result<UserProfileDto>>

    {
        public async Task<Result<UserProfileDto>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            var result = await identity.CreateUserAsync(request.Email, request.FullName, request.TemporaryPassword, request.PreferredLanguage, request.Role, cancellationToken);

            if (result.IsFailure) return result.Error;

            return UserProfileDto.From(result.Value);


        }
    }
}
