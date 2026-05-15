namespace FrangoFrito.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool IsActive,
    string ImageUrl,
    Guid CategoryId,
    string CategoryName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    bool IsActive = true,
    string? ImageUrl = null);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    bool IsActive,
    string? ImageUrl = null);

public sealed record UpdateProductStatusRequest(bool IsActive);
