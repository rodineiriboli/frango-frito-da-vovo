using FrangoFrito.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence;

internal static class DbContextExtensions
{
    public static async Task SaveChangesWithConcurrencyHandlingAsync(
        this DbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
    }
}
