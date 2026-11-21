using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Campaigns.CreateCampaign;
using DonationHub.Application.Features.Campaigns.DeleteCampaign;
using DonationHub.Application.Features.Campaigns.GetCampaignById;
using DonationHub.Application.Features.Campaigns.GetCampaigns;
using DonationHub.Application.Features.Campaigns.GetSuccessStories;
using DonationHub.Application.Features.Campaigns.UpdateCampaign;
using DonationHub.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/campaigns")]
public sealed class CampaignsController(IMediator mediator) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetCampaignsQuery(Request.GetLanguage()), ct));

    [HttpGet("success-stories"), AllowAnonymous]
    public async Task<IActionResult> GetSuccessStories(CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetSuccessStoriesQuery(Request.GetLanguage()), ct));

    [HttpGet("{id:guid}", Name = "GetCampaignById"), AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetCampaignByIdQuery(id, Request.GetLanguage()), ct));

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateCampaignCommand command, CancellationToken ct)
        => this.ToCreatedAtRoute(
            await mediator.Send(command with { UserId = User.GetUserId() }, ct),
            "GetCampaignById", c => new { id = c.Id });

    [HttpPut("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, UpdateCampaignCommand command, CancellationToken ct)
        => this.ToOk(await mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => this.ToNoContent(await mediator.Send(new DeleteCampaignCommand(id), ct));
}