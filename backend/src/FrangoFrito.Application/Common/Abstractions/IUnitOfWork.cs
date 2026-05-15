namespace FrangoFrito.Application.Common.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
