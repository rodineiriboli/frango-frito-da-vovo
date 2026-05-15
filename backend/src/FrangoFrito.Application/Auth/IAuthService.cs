using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Auth;

public interface IAuthService
{
    Task<ApplicationResult<AuthUserDto>> LoginAsync(LoginRequest request);
    Task SignOutAsync();
    Task<ApplicationResult<AuthUserDto>> GetCurrentUserAsync();
}
