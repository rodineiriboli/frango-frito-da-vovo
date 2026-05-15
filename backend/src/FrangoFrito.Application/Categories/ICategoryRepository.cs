using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Categories;

public interface ICategoryRepository
{
    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken);
    void Add(Category category);
    void Remove(Category category);
}
