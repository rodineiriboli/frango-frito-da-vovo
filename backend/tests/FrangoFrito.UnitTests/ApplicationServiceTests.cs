using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Application.Products;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.UnitTests;

public sealed class ApplicationServiceTests
{
    [Fact]
    public async Task DeleteCategory_ShouldRejectCategoryWithProducts()
    {
        var category = new Category("Combos", "Porções completas.");
        var repository = new FakeCategoryRepository
        {
            Category = category,
            HasProducts = true
        };
        var unitOfWork = new FakeUnitOfWork();
        var service = new CategoryService(repository, unitOfWork);

        var result = await service.DeleteAsync(category.Id, CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.Conflict, result.Status);
        Assert.False(repository.WasRemoved);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateProduct_ShouldValidateCategoryExistenceBeforePersisting()
    {
        var categoryRepository = new FakeCategoryRepository { Exists = false };
        var productRepository = new FakeProductRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new ProductService(productRepository, categoryRepository, unitOfWork);

        var result = await service.CreateAsync(
            new CreateProductRequest("Coxa crocante", "Porção com 6 unidades.", 39.90m, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(ApplicationResultStatus.ValidationError, result.Status);
        Assert.False(productRepository.WasAdded);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public Category? Category { get; init; }
        public bool Exists { get; init; } = true;
        public bool HasProducts { get; init; }
        public bool WasRemoved { get; private set; }

        public Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<CategoryDto>>([]);

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Category);

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Exists);

        public Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(HasProducts);

        public void Add(Category category)
        {
        }

        public void Remove(Category category)
        {
            WasRemoved = true;
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public bool WasAdded { get; private set; }

        public Task<PagedResult<ProductDto>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            bool? active,
            Guid? categoryId,
            CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task LoadCategoryAsync(Product product, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public void Add(Product product)
        {
            WasAdded = true;
        }
    }
}
