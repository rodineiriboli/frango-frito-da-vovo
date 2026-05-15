using Microsoft.AspNetCore.Identity;

namespace FrangoFrito.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
