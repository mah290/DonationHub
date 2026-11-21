using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DonationHub.UnitTests;

public class IdentityServiceAuthenticationTests
{
    private readonly Mock<UserManager<ApplicationUser>> _users;
    private readonly IdentityService _sut;

    public IdentityServiceAuthenticationTests()
    {
        _users = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new IdentityService(_users.Object, TestData.NewClock());
    }

    private static ApplicationUser User(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Email = "ali@test.com",
        UserName = "ali@test.com",
        IsActive = isActive
    };

    [Fact]
    public async Task Authenticate_UnknownEmailAndWrongPassword_ReturnTheSameError()
    {
        // Arrange
        var user = User();
        _users.Setup(u => u.FindByEmailAsync("ghost@test.com")).ReturnsAsync((ApplicationUser?)null);
        _users.Setup(u => u.FindByEmailAsync("ali@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "Wrong1pass")).ReturnsAsync(false);

        // Act
        var unknownEmail = await _sut.AuthenticateAsync("ghost@test.com", "Wrong1pass", CancellationToken.None);
        var wrongPassword = await _sut.AuthenticateAsync("ali@test.com", "Wrong1pass", CancellationToken.None);

        // Assert
        unknownEmail.IsFailure.Should().BeTrue();
        wrongPassword.IsFailure.Should().BeTrue();
        unknownEmail.Error.Should().Be(wrongPassword.Error);
        wrongPassword.Error.Code.Should().Be("Auth.InvalidCredentials");
        wrongPassword.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Authenticate_DeactivatedUserWithWrongPassword_DoesNotRevealDeactivation()
    {
        // Arrange
        var user = User(isActive: false);
        _users.Setup(u => u.FindByEmailAsync("ali@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "Wrong1pass")).ReturnsAsync(false);

        // Act
        var result = await _sut.AuthenticateAsync("ali@test.com", "Wrong1pass", CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Authenticate_DeactivatedUserWithCorrectPassword_ReturnsAccountDeactivated()
    {
        // Arrange
        var user = User(isActive: false);
        _users.Setup(u => u.FindByEmailAsync("ali@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "Passw0rd1")).ReturnsAsync(true);

        // Act
        var result = await _sut.AuthenticateAsync("ali@test.com", "Passw0rd1", CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("Auth.AccountDeactivated");
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }
}

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAuthenticationFails_ReturnsTheErrorAndIssuesNoToken()
    {
        // Arrange
        var identity = new Mock<IIdentityService>();
        var tokens = new Mock<IRefreshTokenRepository>();
        var uow = new Mock<IUnitOfWork>();
        identity.Setup(i => i.AuthenticateAsync("ali@test.com", "bad", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserInfo>.Failure(AuthErrors.InvalidCredentials()));

        var sut = new LoginCommandHandler(
                  identity.Object,
                  TestData.NewClock(),
                  new TokenIssuer(Mock.Of<ITokenService>()),
                  uow.Object,
                  tokens.Object);

        // Act
        var result = await sut.Handle(
            new LoginCommand { Email = "ali@test.com", Password = "bad" }, CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
        tokens.Verify(r => r.AddAsync(It.IsAny<DonationHub.Domain.Auth.RefreshToken>(),
            It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}