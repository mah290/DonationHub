using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(IAuthService auth) : ControllerBase
    {
        [HttpPost("register"), AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
        {
            var profile = await auth.RegisterAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, profile);
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
            => Ok(await auth.LoginAsync(request, ct));

        [HttpPost("refresh"), AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
            => Ok(await auth.RefreshAsync(request.RefreshToken, ct));

        [HttpPost("logout"), Authorize]
        public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
        {
            await auth.LogoutAsync(User.GetUserId(), request.RefreshToken, ct);
            return NoContent();
        }

        [HttpGet("me"), Authorize]
        public async Task<IActionResult> Me(CancellationToken ct)
            => Ok(await auth.GetProfileAsync(User.GetUserId(), ct));

        [HttpPut("me"), Authorize]
        public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken ct)
            => Ok(await auth.UpdateProfileAsync(User.GetUserId(), request, ct));

        [HttpPost("change-password"), Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
        {
            await auth.ChangePasswordAsync(User.GetUserId(), request, ct);
            return NoContent();

        }
            
    }
}
