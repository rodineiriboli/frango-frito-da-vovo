using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class CategoriesController : ControllerBase
{
    private readonly FrangoFritoDbContext _dbContext;

    public CategoriesController(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.IsActive,
                category.CreatedAt,
                category.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return category is null ? NotFound() : Ok(category.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.Description);

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        category.Update(request.Name, request.Description);
        if (request.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(category.ToDto());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var hasProducts = await _dbContext.Products.AnyAsync(product => product.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Categoria em uso.",
                Detail = "Categorias vinculadas a produtos nao podem ser excluidas.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
