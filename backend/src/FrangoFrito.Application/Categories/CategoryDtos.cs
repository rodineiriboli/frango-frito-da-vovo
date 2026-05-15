namespace FrangoFrito.Application.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateCategoryRequest(string Name, string Description);

public sealed record UpdateCategoryRequest(string Name, string Description, bool IsActive);
