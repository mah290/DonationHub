using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Admin.CreateStaff;
using DonationHub.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminController(IMediator mediator) : ControllerBase
{
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff(CreateStaffCommand command, CancellationToken ct)
        => this.ToCreated(await mediator.Send(command, ct));
}