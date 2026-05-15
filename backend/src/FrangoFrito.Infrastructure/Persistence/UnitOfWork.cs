using FrangoFrito.Application.Common.Abstractions;

namespace FrangoFrito.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly FrangoFritoDbContext _dbContext;

    public UnitOfWork(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesWithConcurrencyHandlingAsync(cancellationToken);
}
