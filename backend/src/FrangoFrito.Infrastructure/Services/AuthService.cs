using FrangoFrito.Application.Auth;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Security;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FrangoFrito.Infrastructure.Services;

internal sealed class AuthService : IAuthService
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public AuthService(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        ICurrentUser currentUser)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<ApplicationResult<AuthUserDto>> LoginAsync(LoginRequest request)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return ApplicationResult<AuthUserDto>.Unauthorized("Credenciais inválidas.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, request.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return ApplicationResult<AuthUserDto>.Unauthorized("Credenciais inválidas.");
        }

        return ApplicationResult<AuthUserDto>.Success(await ToDtoAsync(user));
    }

    public Task SignOutAsync() => _signInManager.SignOutAsync();

    public async Task<ApplicationResult<AuthUserDto>> GetCurrentUserAsync()
    {
        if (_currentUser.UserId is not { } userId)
        {
            return ApplicationResult<AuthUserDto>.Unauthorized("Sessão expirada ou credenciais inválidas.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null
            ? ApplicationResult<AuthUserDto>.Unauthorized("Sessão expirada ou credenciais inválidas.")
            : ApplicationResult<AuthUserDto>.Success(await ToDtoAsync(user));
    }

    private async Task<AuthUserDto> ToDtoAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUserDto(user.Id, user.FullName, user.Email ?? string.Empty, roles.ToArray());
    }
}
