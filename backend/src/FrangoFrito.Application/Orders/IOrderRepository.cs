using FrangoFrito.Application.Common;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.Enums;

namespace FrangoFrito.Application.Orders;

public enum OrderAccessScope
{
    None,
    All,
    Kitchen,
    Courier
}

public interface IOrderRepository
{
    Task<PagedResult<OrderDto>> GetPagedAsync(
        int page,
        int pageSize,
        OrderStatus? status,
        OrderAccessScope scope,
        CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(Guid id, OrderAccessScope scope, CancellationToken cancellationToken);
    void Add(Order order);
}
