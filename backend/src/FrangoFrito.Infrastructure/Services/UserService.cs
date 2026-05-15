using FrangoFrito.Application.Auth;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Security;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Services;

internal sealed class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;

    public UserService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken)
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

        return result;
    }

    public async Task<ApplicationResult<UserDto>> CreateAsync(CreateUserRequest request)
    {
        if (!AppRoles.All.Contains(request.Role))
        {
            return ApplicationResult<UserDto>.Validation(new Dictionary<string, string[]> { ["role"] = ["Perfil inválido."] });
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
            return ApplicationResult<UserDto>.Validation(new Dictionary<string, string[]>
            {
                ["identity"] = result.Errors.Select(error => error.Description).ToArray()
            });
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        return ApplicationResult<UserDto>.Success(new UserDto(user.Id, user.FullName, user.Email ?? string.Empty, [request.Role]));
    }
}
