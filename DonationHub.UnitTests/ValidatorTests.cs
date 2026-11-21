using DonationHub.Application.Features.Auth.Register;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using FluentAssertions;

namespace DonationHub.UnitTests;

public class CreateCampaignCommandValidatorTests
{
    private readonly CreateCampaignCommandValidator _sut = new();

    private static CreateCampaignCommand Valid() => new()
    {
        UserId = Guid.NewGuid(),
        TitleAr = "حملة",
        TitleEn = "Campaign",
        GoalAmount = 1000m,
        EndDate = TestData.Start.UtcDateTime.AddDays(10),
        CategoryId = Guid.NewGuid()
    };

    [Fact]
    public void ValidCommand_Passes()
    {
        _sut.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingArabicTitle_IsRejected(string title)
    {
        var result = _sut.Validate(Valid() with { TitleAr = title });

        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(CreateCampaignCommand.TitleAr));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingEnglishTitle_IsRejected(string title)
    {
        var result = _sut.Validate(Valid() with { TitleEn = title });

        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(CreateCampaignCommand.TitleEn));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NonPositiveGoal_IsRejected(decimal goal)
    {
        var result = _sut.Validate(Valid() with { GoalAmount = goal });

        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(CreateCampaignCommand.GoalAmount));
    }

    [Fact]
    public void EmptyCategoryId_IsRejected()
    {
        var result = _sut.Validate(Valid() with { CategoryId = Guid.Empty });

        result.Errors.Select(e => e.PropertyName).Should().Contain(nameof(CreateCampaignCommand.CategoryId));
    }
}

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _sut = new();

    private static RegisterCommand WithPassword(string password) => new()
    {
        Email = "ali@test.com",
        FullName = "Ali",
        Password = password,
        PreferredLanguage = "en"
    };

    [Theory]
    [InlineData("")]               // فاضي
    [InlineData("Ab1")]            // أقل من 8
    [InlineData("alllowercase1")]  // مفيش حرف كبير
    [InlineData("ALLUPPERCASE1")]  // مفيش حرف صغير
    [InlineData("NoDigitsHere")]   // مفيش رقم
    public void PasswordBreakingPolicy_IsRejected(string password)
    {
        var result = _sut.Validate(WithPassword(password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void PasswordMeetingPolicy_Passes()
    {
        _sut.Validate(WithPassword("Passw0rd1")).IsValid.Should().BeTrue();
    }
}