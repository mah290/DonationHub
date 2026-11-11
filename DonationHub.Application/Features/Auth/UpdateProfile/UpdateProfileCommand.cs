using System.Text.Json.Serialization;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Users;
using FluentValidation;
using MediatR;

namespace DonationHub.Application.Features.Auth.UpdateProfile;

public sealed record UpdateProfileCommand : IRequest<Result<UserProfileDto>>
{
    [JsonIgnore] public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? PreferredLanguage { get; init; }
}

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PreferredLanguage!)
            .Must(Languages.IsSupported).WithMessage("Preferred language must be 'en' or 'ar'.")
            .When(x => x.PreferredLanguage is not null);
    }
}

internal sealed class UpdateProfileCommandHandler(IIdentityService identity)
    : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        var result = await identity.UpdateProfileAsync(cmd.UserId, cmd.FullName, cmd.PreferredLanguage, ct);
        if (result.IsFailure) return result.Error;

        return UserProfileDto.From(result.Value);
    }
}
