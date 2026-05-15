using FrangoFrito.Application.Common;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Products;

public interface IProductRepository
{
    Task<PagedResult<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? active,
        Guid? categoryId,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task LoadCategoryAsync(Product product, CancellationToken cancellationToken);
    void Add(Product product);
}
