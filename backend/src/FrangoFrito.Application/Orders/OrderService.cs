using FrangoFrito.Application.Common;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Products;
using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.Enums;

namespace FrangoFrito.Application.Orders;

internal sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplicationResult<PagedResult<OrderDto>>> GetAllAsync(int page, int pageSize, string? status, CancellationToken cancellationToken)
    {
        if (!TryParseStatus(status, out var parsedStatus, out var validationResult))
        {
            return validationResult;
        }

        var result = await _orderRepository.GetPagedAsync(page, pageSize, parsedStatus, GetAccessScope(), cancellationToken);
        return ApplicationResult<PagedResult<OrderDto>>.Success(result);
    }

    public async Task<ApplicationResult<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, GetAccessScope(), cancellationToken);
        return order is null
            ? ApplicationResult<OrderDto>.NotFound()
            : ApplicationResult<OrderDto>.Success(order.ToDto());
    }

    public async Task<ApplicationResult<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!CanCreateOrder())
        {
            return ApplicationResult<OrderDto>.Forbidden();
        }

        if (!await _customerRepository.ExistsAsync(request.CustomerId, cancellationToken))
        {
            return ApplicationResult<OrderDto>.Validation(new Dictionary<string, string[]> { ["customerId"] = ["Cliente inexistente."] });
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return ApplicationResult<OrderDto>.Validation(new Dictionary<string, string[]> { ["items"] = ["Pedido deve ter ao menos um item."] });
        }

        var requestedProducts = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
            .ToArray();

        var products = await _productRepository.GetByIdsAsync(requestedProducts.Select(item => item.ProductId), cancellationToken);
        var order = new Order(request.CustomerId);

        foreach (var requestedProduct in requestedProducts)
        {
            if (!products.TryGetValue(requestedProduct.ProductId, out var product))
            {
                return ApplicationResult<OrderDto>.Validation(new Dictionary<string, string[]> { ["items"] = [$"Produto {requestedProduct.ProductId} inexistente."] });
            }

            order.AddItem(product, requestedProduct.Quantity);
        }

        order.EnsureHasItems();

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _orderRepository.GetByIdAsync(order.Id, OrderAccessScope.All, cancellationToken);
        return ApplicationResult<OrderDto>.Success(created!.ToDto());
    }

    public async Task<ApplicationResult<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            return InvalidStatus<OrderDto>();
        }

        if (!CanChangeTo(newStatus))
        {
            return ApplicationResult<OrderDto>.Forbidden();
        }

        var order = await _orderRepository.GetByIdAsync(id, GetAccessScope(), cancellationToken);
        if (order is null)
        {
            return ApplicationResult<OrderDto>.NotFound();
        }

        order.ChangeStatus(newStatus);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApplicationResult<OrderDto>.Success(order.ToDto());
    }

    private bool TryParseStatus(
        string? status,
        out OrderStatus? parsedStatus,
        out ApplicationResult<PagedResult<OrderDto>> validationResult)
    {
        parsedStatus = null;
        validationResult = default!;

        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        if (Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var value))
        {
            parsedStatus = value;
            return true;
        }

        validationResult = InvalidStatus<PagedResult<OrderDto>>();
        return false;
    }

    private OrderAccessScope GetAccessScope()
    {
        if (_currentUser.IsInRole(AppRoles.Admin) || _currentUser.IsInRole(AppRoles.Attendant))
        {
            return OrderAccessScope.All;
        }

        if (_currentUser.IsInRole(AppRoles.Kitchen))
        {
            return OrderAccessScope.Kitchen;
        }

        return _currentUser.IsInRole(AppRoles.Courier) ? OrderAccessScope.Courier : OrderAccessScope.None;
    }

    private bool CanCreateOrder() =>
        _currentUser.IsInRole(AppRoles.Admin) || _currentUser.IsInRole(AppRoles.Attendant);

    private bool CanChangeTo(OrderStatus targetStatus)
    {
        if (_currentUser.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        if (_currentUser.IsInRole(AppRoles.Attendant))
        {
            return targetStatus == OrderStatus.Cancelled;
        }

        if (_currentUser.IsInRole(AppRoles.Kitchen))
        {
            return targetStatus == OrderStatus.Preparing;
        }

        if (_currentUser.IsInRole(AppRoles.Courier))
        {
            return targetStatus is OrderStatus.OutForDelivery or OrderStatus.Delivered;
        }

        return false;
    }

    private static ApplicationResult<T> InvalidStatus<T>() =>
        ApplicationResult<T>.Validation(new Dictionary<string, string[]> { ["status"] = ["Status inválido."] });
}
