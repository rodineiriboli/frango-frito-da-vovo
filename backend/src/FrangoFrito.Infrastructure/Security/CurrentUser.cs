using System.Security.Claims;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Http;

namespace FrangoFrito.Infrastructure.Security;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var rawUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(rawUserId, out var userId) ? userId : null;
        }
    }

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
