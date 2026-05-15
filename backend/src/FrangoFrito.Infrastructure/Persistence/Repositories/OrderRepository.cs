using FrangoFrito.Application.Common;
using FrangoFrito.Application.Orders;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly FrangoFritoDbContext _dbContext;

    public OrderRepository(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(
        int page,
        int pageSize,
        OrderStatus? status,
        OrderAccessScope scope,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .AsQueryable();

        query = ApplyScope(query, scope);

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        var paging = new PagedQuery(page, pageSize);
        var totalItems = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.CreatedAt)
            .Skip((paging.SafePage - 1) * paging.SafePageSize)
            .Take(paging.SafePageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<OrderDto>(
            orders.Select(order => order.ToDto()).ToArray(),
            paging.SafePage,
            paging.SafePageSize,
            totalItems);
    }

    public Task<Order?> GetByIdAsync(Guid id, OrderAccessScope scope, CancellationToken cancellationToken) =>
        ApplyScope(_dbContext.Orders
                .Include(item => item.Customer)
                .Include(item => item.Items)
                .AsQueryable(),
            scope)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public void Add(Order order) => _dbContext.Orders.Add(order);

    private static IQueryable<Order> ApplyScope(IQueryable<Order> query, OrderAccessScope scope) =>
        scope switch
        {
            OrderAccessScope.All => query,
            OrderAccessScope.Kitchen => query.Where(order => order.Status == OrderStatus.Received || order.Status == OrderStatus.Preparing),
            OrderAccessScope.Courier => query.Where(order => order.Status == OrderStatus.OutForDelivery || order.Status == OrderStatus.Delivered),
            _ => query.Where(order => false)
        };
}
