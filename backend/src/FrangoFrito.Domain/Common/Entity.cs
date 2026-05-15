namespace FrangoFrito.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public void MarkUpdated(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version = Guid.NewGuid();
    }
}
