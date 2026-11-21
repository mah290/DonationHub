using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using FluentAssertions;
using FluentValidation;

namespace DonationHub.UnitTests;

public class ValidationPipelineTests
{
    private static ValidationBehavior<CreateCampaignCommand, Result<CampaignAdminDto>> Behavior() =>
        new(new IValidator<CreateCampaignCommand>[] { new CreateCampaignCommandValidator() });

    [Fact]
    public async Task InvalidRequest_ShortCircuitsWithValidationFailed_AndNeverInvokesHandler()
    {
        // Arrange
        var invalid = new CreateCampaignCommand();   // كل الحقول فاضية
        var handlerCalls = 0;

        // Act
        var result = await Behavior().Handle(invalid, _ =>
        {
            handlerCalls++;
            return Task.FromResult(Result<CampaignAdminDto>.Success(null!));
        }, CancellationToken.None);

        // Assert
        handlerCalls.Should().Be(0);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Failed");
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.ValidationErrors.Should().NotBeNull();
        result.Error.ValidationErrors!.Keys.Should()
            .Contain(["titleAr", "titleEn", "goalAmount", "categoryId"]);   // camelCase
    }

    [Fact]
    public async Task ValidRequest_ReachesHandler()
    {
        // Arrange
        var valid = new CreateCampaignCommand
        {
            UserId = Guid.NewGuid(),
            TitleAr = "حملة",
            TitleEn = "Campaign",
            GoalAmount = 1000m,
            EndDate = TestData.Start.UtcDateTime.AddDays(10),
            CategoryId = Guid.NewGuid()
        };
        var handlerCalls = 0;

        // Act
        await Behavior().Handle(valid, _ =>
        {
            handlerCalls++;
            return Task.FromResult(Result<CampaignAdminDto>.Success(null!));
        }, CancellationToken.None);

        // Assert
        handlerCalls.Should().Be(1);
    }
}