using FrangoFrito.Application.Menu;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/menu")]
public sealed class MenuController : ControllerBase
{
    private readonly FrangoFritoDbContext _dbContext;

    public MenuController(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MenuCategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Include(category => category.Products.Where(product => product.IsActive))
            .OrderBy(category => category.Name)
            .ToArrayAsync(cancellationToken);

        var menu = categories
            .Select(category => new MenuCategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.Products
                    .Where(product => product.IsActive)
                    .OrderBy(product => product.Name)
                    .Select(product => new MenuProductDto(product.Id, product.Name, product.Description, product.Price, product.ImageUrl))
                    .ToArray()))
            .ToArray();

        return Ok(menu);
    }
}
