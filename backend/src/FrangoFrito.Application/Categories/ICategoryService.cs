using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ApplicationResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
