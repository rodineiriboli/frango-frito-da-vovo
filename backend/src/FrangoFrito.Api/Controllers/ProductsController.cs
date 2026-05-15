using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Products;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
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
        var result = await _productService.GetAllAsync(page, pageSize, search, active, categoryId, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<ActionResult<ProductDto>> UpdateStatus(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateStatusAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
