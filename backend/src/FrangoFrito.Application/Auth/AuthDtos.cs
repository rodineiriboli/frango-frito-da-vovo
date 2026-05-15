namespace FrangoFrito.Application.Auth;

public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);

public sealed record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles);

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string Role);

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles);
