using DonationHub.Api.Authorization;
using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Application.Features.Campaigns.DeleteCampaign;
using DonationHub.Application.Features.Campaigns.UpdateCampaign;
using DonationHub.Domain.Campaigns;
using DonationHub.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace DonationHub.UnitTests
{
    public class CampaignOwnershipHandlerTests
    {
        private static readonly DateTime Now = TestData.Start.UtcDateTime;

        private static Campaign OwnedBy(Guid ownerId) =>
            Campaign.Create("ع", "T", "", "", 100m, Now.AddDays(5), Guid.NewGuid(), ownerId, Now);

        private static ClaimsPrincipal User(string role, Guid id) =>
            new(new ClaimsIdentity([new Claim("sub", id.ToString()), new Claim("role", role)],
                "test", nameType: "sub", roleType: "role"));

        private static async Task<bool> Succeeds(ClaimsPrincipal user, Campaign campaign)
        {
            var context = new AuthorizationHandlerContext(
                [new CampaignOwnershipRequirement()], user, campaign);
            await new CampaignOwnershipHandler().HandleAsync(context);
            return context.HasSucceeded;
        }

        [Fact]
        public async Task Manager_OnOwnCampaign_Succeeds()
        {
            var id = Guid.NewGuid();
            (await Succeeds(User(Roles.CampaignManager, id), OwnedBy(id))).Should().BeTrue();
        }

        [Fact]
        public async Task Manager_OnSomeoneElsesCampaign_Fails()
        {
            (await Succeeds(User(Roles.CampaignManager, Guid.NewGuid()), OwnedBy(Guid.NewGuid())))
                .Should().BeFalse();
        }

        [Fact]
        public async Task Admin_OnAnyCampaign_Succeeds()
        {
            (await Succeeds(User(Roles.Admin, Guid.NewGuid()), OwnedBy(Guid.NewGuid()))).Should().BeTrue();
        }

        [Fact]
        public async Task Donor_OnAnyCampaign_Fails()
        {
            var id = Guid.NewGuid();
            (await Succeeds(User(Roles.Donor, id), OwnedBy(id))).Should().BeFalse();
        }
    }

    public class OwnershipInHandlersTests
    {
        private readonly Mock<ICampaignRepository> _campaigns = new();
        private readonly Mock<ICampaignAuthorizer> _authorizer = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private Campaign NotMine()
        {
            var now = TestData.Start.UtcDateTime;
            var campaign = TestData.NewCampaign(now);
            _campaigns.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
            _authorizer.Setup(a => a.CanManageAsync(campaign, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            return campaign;
        }

        [Fact]
        public async Task Update_OnCampaignOfAnotherUser_Returns404NotFoundAndNeverSaves()
        {
            var campaign = NotMine();
            var sut = new UpdateCampaignCommandHandler(
                _campaigns.Object, Mock.Of<DonationHub.Application.Features.Categories.ICategoryRepository>(),
                _authorizer.Object, _uow.Object, TestData.NewClock());

            var result = await sut.Handle(new UpdateCampaignCommand
            {
                Id = campaign.Id,
                TitleAr = "ع",
                TitleEn = "T",
                GoalAmount = 100m,
                EndDate = campaign.EndDate,
                CategoryId = Guid.NewGuid(),
                RowVersion = [1]
            }, CancellationToken.None);

            result.Error.Code.Should().Be("Campaign.NotFound");   // مش Forbidden
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Delete_OnCampaignOfAnotherUser_Returns404AndDoesNotDelete()
        {
            var campaign = NotMine();
            var sut = new DeleteCampaignCommandHandler(
                _campaigns.Object, _authorizer.Object, _uow.Object, TestData.NewClock());

            var result = await sut.Handle(new DeleteCampaignCommand(campaign.Id), CancellationToken.None);

            result.Error.Code.Should().Be("Campaign.NotFound");
            campaign.IsDeleted.Should().BeFalse();
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }


}
