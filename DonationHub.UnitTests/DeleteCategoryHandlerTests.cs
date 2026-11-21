using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Features.Categories;
using DonationHub.Application.Features.Categories.DeleteCategory;
using DonationHub.Application.Common.Results;
using DonationHub.Domain.Categories;
using FluentAssertions;
using Moq;

namespace DonationHub.UnitTests;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly DeleteCategoryCommandHandler _sut;

    public DeleteCategoryHandlerTests() => _sut = new(_categories.Object, _uow.Object);

    private Category ExistingCategory()
    {
        var category = Category.Create("صحة", "Health", "", "", TestData.Start.UtcDateTime);
        _categories.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        return category;
    }

    [Fact]
    public async Task Handle_CategoryWithCampaigns_ReturnsHasCampaignsAndDeletesNothing()
    {
        // Arrange
        var category = ExistingCategory();
        _categories.Setup(r => r.HasCampaignsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.HasCampaigns");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        _categories.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CategoryWithoutCampaigns_RemovesAndSavesOnce()
    {
        // Arrange
        var category = ExistingCategory();
        _categories.Setup(r => r.HasCampaignsAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _categories.Verify(r => r.Remove(category), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}