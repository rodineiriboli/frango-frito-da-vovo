using FrangoFrito.Application.Common;
using FrangoFrito.Application.Products;
using FrangoFrito.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly FrangoFritoDbContext _dbContext;

    public ProductRepository(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? active,
        Guid? categoryId,
        CancellationToken cancellationToken)
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

        return await query
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
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
        await _dbContext.Products
            .Where(product => ids.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

    public Task LoadCategoryAsync(Product product, CancellationToken cancellationToken) =>
        _dbContext.Entry(product).Reference(item => item.Category).LoadAsync(cancellationToken);

    public void Add(Product product) => _dbContext.Products.Add(product);
}
