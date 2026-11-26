using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Common.Exceptions;
using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Application.Features.Campaigns.UpdateCampaign;
using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Categories;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace DonationHub.UnitTests;

public class UpdateCampaignCommandHandlerTests
{
    private readonly Mock<ICampaignRepository> _campaigns = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly FakeTimeProvider _clock = TestData.NewClock();
    private readonly UpdateCampaignCommandHandler _sut;
    private readonly Mock<ICampaignAuthorizer> _authorizer = new();

    public UpdateCampaignCommandHandlerTests()
    {
        _authorizer.Setup(a => a.CanManageAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _sut = new(_campaigns.Object, _categories.Object, _authorizer.Object, _uow.Object, _clock);
    }

    //public UpdateCampaignCommandHandlerTests() =>
    //    _sut = new(_campaigns.Object, _categories.Object, _uow.Object, _clock);

    // 13) id مش موجود
    [Fact]
    public async Task Handle_UnknownId_ReturnsCampaignNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _campaigns.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        var command = new UpdateCampaignCommand
        {
            Id = id,
            TitleAr = "ع",
            TitleEn = "T",
            GoalAmount = 100m,
            EndDate = _clock.GetUtcNow().UtcDateTime.AddDays(5),
            CategoryId = Guid.NewGuid(),
            RowVersion = [1]
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Campaign.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // 14) تعارض التزامن
    [Fact]
    public async Task Handle_WhenSaveDetectsConcurrentEdit_PipelineReturnsConcurrencyConflict()
    {
        // Arrange
        var now = _clock.GetUtcNow().UtcDateTime;
        var campaign = TestData.NewCampaign(now);
        var category = Category.Create("صحة", "Health", "", "", now);
        byte[] rowVersion = [1, 2, 3];

        _campaigns.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        // UnitOfWork الحقيقي بيترجم DbUpdateConcurrencyException لـ ConcurrencyException
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyException());

        var command = new UpdateCampaignCommand
        {
            Id = campaign.Id,
            TitleAr = "ع",
            TitleEn = "T",
            GoalAmount = 1000m,
            EndDate = campaign.EndDate,
            CategoryId = category.Id,
            RowVersion = rowVersion
        };
        var behavior = new ConcurrencyBehavior<UpdateCampaignCommand, Result<CampaignAdminDto>>();

        // Act
        var result = await behavior.Handle(
            command, token => _sut.Handle(command, token), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Campaign.ConcurrencyConflict");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        // الـ RowVersion اللي العميل بعته اتحط فعلاً كـ OriginalValue (CMP-10)
        _campaigns.Verify(r => r.SetExpectedRowVersion(campaign, rowVersion), Times.Once);
    }
}