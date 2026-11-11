using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Auth.ChangePassword;
using DonationHub.Application.Features.Auth.GetMe;
using DonationHub.Application.Features.Auth.Login;
using DonationHub.Application.Features.Auth.Logout;
using DonationHub.Application.Features.Auth.RefreshTokens;
using DonationHub.Application.Features.Auth.Register;
using DonationHub.Application.Features.Auth.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
        => this.ToCreated(await mediator.Send(command, ct));

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        => this.ToOk(await mediator.Send(command, ct));

    [HttpPost("refresh"), AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken ct)
        => this.ToOk(await mediator.Send(command, ct));

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken ct)
        => this.ToNoContent(await mediator.Send(command with { UserId = User.GetUserId() }, ct));

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetMeQuery(User.GetUserId()), ct));

    [HttpPut("me"), Authorize]
    public async Task<IActionResult> UpdateMe(UpdateProfileCommand command, CancellationToken ct)
        => this.ToOk(await mediator.Send(command with { UserId = User.GetUserId() }, ct));

    [HttpPost("change-password"), Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken ct)
        => this.ToNoContent(await mediator.Send(command with { UserId = User.GetUserId() }, ct));
}