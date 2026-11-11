using DonationHub.Api.Extensions;
using DonationHub.Application.Features.Categories.CreateCategory;
using DonationHub.Application.Features.Categories.DeleteCategory;
using DonationHub.Application.Features.Categories.GetCategories;
using DonationHub.Application.Features.Categories.GetCategoryById;
using DonationHub.Application.Features.Categories.UpdateCategory;
using DonationHub.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetCategoriesQuery(), ct));

    [HttpGet("{id:guid}", Name = "GetCategoryById"), AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => this.ToOk(await mediator.Send(new GetCategoryByIdQuery(id), ct));

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken ct)
        => this.ToCreatedAtRoute(await mediator.Send(command, ct), "GetCategoryById", c => new { id = c.Id });

    [HttpPut("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command, CancellationToken ct)
        => this.ToOk(await mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => this.ToNoContent(await mediator.Send(new DeleteCategoryCommand(id), ct));
}