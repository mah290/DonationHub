using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Categories;
using DonationHub.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace DonationHub.UnitTests;

public class CreateCampaignCommandHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaigns = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private readonly CreateCampaignCommandHandler _sut;

    public CreateCampaignCommandHandlerTests() =>
        _sut = new(_campaigns.Object, _categories.Object, _uow.Object, _clock);

    private CreateCampaignCommand Command(Guid categoryId) => new()
    {
        UserId = Guid.NewGuid(),
        TitleAr = "حملة",
        TitleEn = "Campaign",
        DescriptionAr = "وصف",
        DescriptionEn = "Description",
        GoalAmount = 1000m,
        EndDate = _clock.GetUtcNow().UtcDateTime.AddDays(10),
        CategoryId = categoryId
    };

    // 11) الـ category مش موجودة
    [Fact]
    public async Task Handle_UnknownCategory_ReturnsCategoryNotFoundAndNeverSaves()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categories.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.Handle(Command(categoryId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
        _campaigns.Verify(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 12) النجاح
    [Fact]
    public async Task Handle_ValidCommand_AddsCampaignAndSavesOnce()
    {
        // Arrange
        var category = Category.Create("صحة", "Health", "", "", _clock.GetUtcNow().UtcDateTime);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var command = Command(category.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CampaignStatus.Active);
        result.Value.RaisedAmount.Should().Be(0m);
        result.Value.CreatedByUserId.Should().Be(command.UserId);

        _campaigns.Verify(r => r.AddAsync(
            It.Is<Campaign>(c => c.CategoryId == category.Id && c.TitleEn == "Campaign"),
            It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}