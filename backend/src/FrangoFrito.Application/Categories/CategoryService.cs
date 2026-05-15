using FrangoFrito.Application.Common;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Categories;

internal sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        _categoryRepository.GetAllAsync(cancellationToken);

    public async Task<ApplicationResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        return category is null
            ? ApplicationResult<CategoryDto>.NotFound()
            : ApplicationResult<CategoryDto>.Success(category.ToDto());
    }

    public async Task<ApplicationResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.Description);

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApplicationResult<CategoryDto>.Success(category.ToDto());
    }

    public async Task<ApplicationResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return ApplicationResult<CategoryDto>.NotFound();
        }

        category.Update(request.Name, request.Description);
        if (request.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApplicationResult<CategoryDto>.Success(category.ToDto());
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return ApplicationResult.NotFound();
        }

        if (await _categoryRepository.HasProductsAsync(id, cancellationToken))
        {
            return ApplicationResult.Conflict(
                "Categoria em uso.",
                "Categorias vinculadas a produtos não podem ser excluídas.");
        }

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success();
    }
}
