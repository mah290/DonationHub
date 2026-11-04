using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers
{
    [Route("api/v1/Categories")]
    [ApiController]
    public sealed class CategoriesController(CategoryService categoryService) : ControllerBase
    {
        [HttpGet,AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            return Ok(await categoryService.GetAllAsync(ct));
        }

        [HttpGet("{id:guid}", Name = "GetCategoryById"), AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            return Ok(await categoryService.GetByIdAsync(id, ct));
        }

        [HttpPost , Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CreateCategory(CategoryRequest request, CancellationToken ct)
        {
            var created = await categoryService.CreateAsync(request, ct);
            return CreatedAtRoute("GetCategoryById", new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}"), Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateCategory(Guid id, CategoryRequest request, CancellationToken ct)
        {
            return Ok(await categoryService.UpdateAsync(id, request, ct));
        }
        [HttpDelete("{id:guid}"), Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await categoryService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
