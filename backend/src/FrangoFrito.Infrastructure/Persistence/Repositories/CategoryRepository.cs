using FrangoFrito.Application.Categories;
using FrangoFrito.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly FrangoFritoDbContext _dbContext;

    public CategoryRepository(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Categories
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

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Categories.AnyAsync(category => category.Id == id, cancellationToken);

    public Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Products.AnyAsync(product => product.CategoryId == id, cancellationToken);

    public void Add(Category category) => _dbContext.Categories.Add(category);

    public void Remove(Category category) => _dbContext.Categories.Remove(category);
}
