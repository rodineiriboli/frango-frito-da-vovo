using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
