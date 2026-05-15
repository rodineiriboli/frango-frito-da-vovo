namespace FrangoFrito.Application.Menu;

internal sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public Task<IReadOnlyCollection<MenuCategoryDto>> GetAsync(CancellationToken cancellationToken) =>
        _menuRepository.GetAsync(cancellationToken);
}
