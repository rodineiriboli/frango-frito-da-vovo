using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Orders;

public interface IOrderService
{
    Task<ApplicationResult<PagedResult<OrderDto>>> GetAllAsync(int page, int pageSize, string? status, CancellationToken cancellationToken);
    Task<ApplicationResult<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationResult<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken);
}
