using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Products;

internal sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<ProductDto>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        bool? active,
        Guid? categoryId,
        CancellationToken cancellationToken) =>
        _productRepository.GetPagedAsync(page, pageSize, search, active, categoryId, cancellationToken);

    public async Task<ApplicationResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        return product is null
            ? ApplicationResult<ProductDto>.NotFound()
            : ApplicationResult<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApplicationResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return CategoryNotFound();
        }

        var product = new Product(request.Name, request.Description, request.Price, request.CategoryId, request.IsActive, request.ImageUrl);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _productRepository.LoadCategoryAsync(product, cancellationToken);

        return ApplicationResult<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApplicationResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        if (product is null)
        {
            return ApplicationResult<ProductDto>.NotFound();
        }

        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return CategoryNotFound();
        }

        product.Update(request.Name, request.Description, request.Price, request.CategoryId, request.IsActive, request.ImageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _productRepository.LoadCategoryAsync(product, cancellationToken);

        return ApplicationResult<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApplicationResult<ProductDto>> UpdateStatusAsync(Guid id, UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id, cancellationToken);
        if (product is null)
        {
            return ApplicationResult<ProductDto>.NotFound();
        }

        if (request.IsActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApplicationResult<ProductDto>.Success(product.ToDto());
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return ApplicationResult.NotFound();
        }

        product.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success();
    }

    private static ApplicationResult<ProductDto> CategoryNotFound() =>
        ApplicationResult<ProductDto>.Validation(new Dictionary<string, string[]> { ["categoryId"] = ["Categoria inexistente."] });
}
