using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        bool? active,
        Guid? categoryId,
        CancellationToken cancellationToken);

    Task<ApplicationResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult<ProductDto>> UpdateStatusAsync(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
