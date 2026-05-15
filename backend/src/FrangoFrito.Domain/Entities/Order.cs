using FrangoFrito.Domain.Common;
using FrangoFrito.Domain.Enums;

namespace FrangoFrito.Domain.Entities;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public Order(Guid customerId)
    {
        CustomerId = customerId == Guid.Empty ? throw new DomainException("Cliente e obrigatorio.") : customerId;
        Status = OrderStatus.Received;
    }

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.Total);
    public int TotalItems => _items.Sum(item => item.Quantity);

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Received)
        {
            throw new DomainException("Itens so podem ser alterados em pedidos recebidos.");
        }

        var existingItem = _items.FirstOrDefault(item => item.ProductId == product.Id);
        if (existingItem is null)
        {
            _items.Add(new OrderItem(product, quantity));
            return;
        }

        existingItem.Increase(quantity);
    }

    public void EnsureHasItems()
    {
        if (_items.Count == 0)
        {
            throw new DomainException("Pedido deve ter ao menos um item.");
        }
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new DomainException($"Transicao de status invalida: {Status} para {newStatus}.");
        }

        Status = newStatus;
    }

    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> AllowedTransitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Received] = [OrderStatus.Preparing, OrderStatus.Cancelled],
            [OrderStatus.Preparing] = [OrderStatus.OutForDelivery, OrderStatus.Cancelled],
            [OrderStatus.OutForDelivery] = [OrderStatus.Delivered, OrderStatus.Cancelled],
            [OrderStatus.Delivered] = [],
            [OrderStatus.Cancelled] = []
        };
}
