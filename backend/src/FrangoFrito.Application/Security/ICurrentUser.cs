namespace FrangoFrito.Application.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsInRole(string role);
}
