using FrangoFrito.Domain.Common;

namespace FrangoFrito.Domain.Entities;

public sealed class OrderItem : Entity
{
    private OrderItem()
    {
    }

    internal OrderItem(Product product, int quantity)
    {
        if (!product.IsActive)
        {
            throw new DomainException("Produto inativo nao pode ser adicionado ao pedido.");
        }

        ProductId = product.Id;
        ProductName = product.Name;
        UnitPrice = product.Price;
        Increase(quantity);
    }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Total => UnitPrice * Quantity;

    internal void Increase(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantidade do item deve ser maior que zero.");
        }

        Quantity += quantity;
    }
}
