using FrangoFrito.Application.Menu;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence.Repositories;

internal sealed class MenuRepository : IMenuRepository
{
    private readonly FrangoFritoDbContext _dbContext;

    public MenuRepository(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MenuCategoryDto>> GetAsync(CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Include(category => category.Products.Where(product => product.IsActive))
            .OrderBy(category => category.Name)
            .ToArrayAsync(cancellationToken);

        return categories
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
    }
}
