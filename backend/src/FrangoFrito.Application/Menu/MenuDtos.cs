namespace FrangoFrito.Application.Menu;

public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<MenuProductDto> Products);

public sealed record MenuProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string ImageUrl);
