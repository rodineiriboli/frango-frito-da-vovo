using FrangoFrito.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence;

internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQuery paging,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((paging.SafePage - 1) * paging.SafePageSize)
            .Take(paging.SafePageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<T>(items, paging.SafePage, paging.SafePageSize, totalItems);
    }
}
