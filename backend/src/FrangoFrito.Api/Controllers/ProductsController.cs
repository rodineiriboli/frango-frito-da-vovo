using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Products;
using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class ProductsController : ControllerBase
{
    private readonly FrangoFritoDbContext _dbContext;

    public ProductsController(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? active = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(product =>
                EF.Functions.ILike(product.Name, term) ||
                EF.Functions.ILike(product.Description, term));
        }

        if (active.HasValue)
        {
            query = query.Where(product => product.IsActive == active.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        var result = await query
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.IsActive,
                product.ImageUrl,
                product.CategoryId,
                product.Category == null ? string.Empty : product.Category.Name,
                product.CreatedAt,
                product.UpdatedAt))
            .ToPagedResultAsync(new PagedQuery(page, pageSize, search), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return product is null ? NotFound() : Ok(product.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Categories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return this.ValidationErrors(new Dictionary<string, string[]> { ["categoryId"] = ["Categoria inexistente."] });
        }

        var product = new Product(request.Name, request.Description, request.Price, request.CategoryId, request.IsActive, request.ImageUrl);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(product).Reference(item => item.Category).LoadAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        if (!await _dbContext.Categories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return this.ValidationErrors(new Dictionary<string, string[]> { ["categoryId"] = ["Categoria inexistente."] });
        }

        product.Update(request.Name, request.Description, request.Price, request.CategoryId, request.IsActive, request.ImageUrl);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(product).Reference(item => item.Category).LoadAsync(cancellationToken);

        return Ok(product.ToDto());
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> UpdateStatus(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        if (request.IsActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(product.ToDto());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
