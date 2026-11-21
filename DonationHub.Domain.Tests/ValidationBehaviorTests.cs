using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Application.Features.Auth.Register;
using DonationHub.Application.Features.Categories;
using DonationHub.Application.Features.Categories.DeleteCategory;

namespace DonationHub.Domain.Tests;

public class ValidationBehaviorTests
{
    private static RegisterCommand InvalidRegister() => new()
    {
        Email = "not-an-email",
        FullName = "",
        Password = "Passw0rd1",
        PreferredLanguage = "fr"
    };

    [Fact]
    public async Task InvalidRequest_ShortCircuits_AndReturnsPropertyErrors()
    {
        var behavior = new ValidationBehavior<RegisterCommand, Result<UserProfileDto>>(
            [new RegisterCommandValidator()]);
        var handlerCalled = false;

        var result = await behavior.Handle(InvalidRegister(), _ =>
        {
            handlerCalled = true;
            return Task.FromResult(Result<UserProfileDto>.Success(null!));
        }, CancellationToken.None);

        Assert.False(handlerCalled);                       // الـ handler ماتنفذش
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Failed", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        var errors = result.Error.ValidationErrors!;
        Assert.Contains("email", errors.Keys);             // camelCase
        Assert.Contains("fullName", errors.Keys);
        Assert.Contains("preferredLanguage", errors.Keys);
        Assert.DoesNotContain("password", errors.Keys);    // الباسورد سليم
    }

    [Fact]
    public async Task ValidRequest_ReachesHandler()
    {
        var behavior = new ValidationBehavior<RegisterCommand, Result<UserProfileDto>>(
            [new RegisterCommandValidator()]);
        var valid = new RegisterCommand
        {
            Email = "ali@test.com",
            FullName = "Ali",
            Password = "Passw0rd1",
            PreferredLanguage = "ar"
        };
        var handlerCalled = false;

        await behavior.Handle(valid, _ =>
        {
            handlerCalled = true;
            return Task.FromResult(Result<UserProfileDto>.Success(null!));
        }, CancellationToken.None);

        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task RequestWithoutValidators_PassesThrough()
    {
        var behavior = new ValidationBehavior<DeleteCategoryCommand, Result>([]);
        var handlerCalled = false;


        await behavior.Handle(new DeleteCategoryCommand(Guid.NewGuid()), _ =>
        {
            handlerCalled = true;
            return Task.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.True(handlerCalled);
    }

    [Fact]
    public void EmptyId_FailsValidation()
    {
        var result = new DeleteCategoryCommandValidator()
            .Validate(new DeleteCategoryCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}