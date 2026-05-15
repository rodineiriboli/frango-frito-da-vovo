using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var result = await _authService.GetCurrentUserAsync();
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }
}
