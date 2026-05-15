using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Auth;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetAll), result.Value)
            : result.ToActionResult(this);
    }
}
