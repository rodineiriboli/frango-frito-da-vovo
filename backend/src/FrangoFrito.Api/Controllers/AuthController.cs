using FrangoFrito.Application.Auth;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return UnauthorizedProblem();
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return UnauthorizedProblem();
        }

        return Ok(await ToDtoAsync(user));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        return user is null ? UnauthorizedProblem() : Ok(await ToDtoAsync(user));
    }

    private async Task<AuthUserDto> ToDtoAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUserDto(user.Id, user.FullName, user.Email ?? string.Empty, roles.ToArray());
    }

    private ActionResult UnauthorizedProblem() =>
        Problem(title: "Credenciais invalidas.", statusCode: StatusCodes.Status401Unauthorized);
}
