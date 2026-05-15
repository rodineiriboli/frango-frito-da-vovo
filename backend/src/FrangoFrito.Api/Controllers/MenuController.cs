using FrangoFrito.Application.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/menu")]
public sealed class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MenuCategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var menu = await _menuService.GetAsync(cancellationToken);
        return Ok(menu);
    }
}
