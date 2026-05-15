namespace FrangoFrito.Application.Orders;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Total);

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Status,
    decimal Total,
    int TotalItems,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record UpdateOrderStatusRequest(string Status);
