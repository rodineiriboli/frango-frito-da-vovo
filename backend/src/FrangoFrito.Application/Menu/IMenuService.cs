namespace FrangoFrito.Application.Menu;

public interface IMenuService
{
    Task<IReadOnlyCollection<MenuCategoryDto>> GetAsync(CancellationToken cancellationToken);
}
