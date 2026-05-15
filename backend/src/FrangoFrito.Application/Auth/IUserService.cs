using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Auth;

public interface IUserService
{
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ApplicationResult<UserDto>> CreateAsync(CreateUserRequest request);
}
