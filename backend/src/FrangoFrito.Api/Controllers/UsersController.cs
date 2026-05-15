using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Auth;
using FrangoFrito.Application.Security;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .OrderBy(user => user.FullName)
            .ToArrayAsync(cancellationToken);

        var result = new List<UserDto>(users.Length);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto(user.Id, user.FullName, user.Email ?? string.Empty, roles.ToArray()));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        if (!AppRoles.All.Contains(request.Role))
        {
            return this.ValidationErrors(new Dictionary<string, string[]> { ["role"] = ["Perfil invalido."] });
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return this.ValidationErrors(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            });
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        return CreatedAtAction(nameof(GetAll), new UserDto(user.Id, user.FullName, user.Email ?? string.Empty, [request.Role]));
    }
}
