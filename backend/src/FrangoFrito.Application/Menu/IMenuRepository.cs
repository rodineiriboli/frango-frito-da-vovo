namespace FrangoFrito.Application.Menu;

public interface IMenuRepository
{
    Task<IReadOnlyCollection<MenuCategoryDto>> GetAsync(CancellationToken cancellationToken);
}
