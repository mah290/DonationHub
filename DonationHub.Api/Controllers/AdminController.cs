using DonationHub.Application.Features.Admin;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController(IStaffService staff) : ControllerBase
    {
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff(CreateStaffRequest request, CancellationToken ct)
        {
            var profile = await staff.CreateStaffAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, profile);
        }
    }
}
