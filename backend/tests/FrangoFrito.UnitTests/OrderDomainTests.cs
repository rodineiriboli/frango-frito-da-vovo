using FrangoFrito.Domain.Common;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.Enums;

namespace FrangoFrito.UnitTests;

public sealed class OrderDomainTests
{
    [Fact]
    public void AddItem_ShouldRejectInactiveProduct()
    {
        var product = new Product("Coxa crocante", "Porção", 29.90m, Guid.NewGuid(), isActive: false);
        var order = new Order(Guid.NewGuid());

        Assert.Throws<DomainException>(() => order.AddItem(product, 1));
    }

    [Fact]
    public void AddItem_ShouldMergeRepeatedProductAndCalculateTotal()
    {
        var product = new Product("Balde da Vovó", "Combo", 79.90m, Guid.NewGuid());
        var order = new Order(Guid.NewGuid());

        order.AddItem(product, 1);
        order.AddItem(product, 2);

        Assert.Single(order.Items);
        Assert.Equal(3, order.TotalItems);
        Assert.Equal(239.70m, order.Total);
    }

    [Fact]
    public void ChangeStatus_ShouldRejectBackwardTransitionAfterDelivered()
    {
        var order = new Order(Guid.NewGuid());

        order.ChangeStatus(OrderStatus.Preparing);
        order.ChangeStatus(OrderStatus.OutForDelivery);
        order.ChangeStatus(OrderStatus.Delivered);

        Assert.Throws<DomainException>(() => order.ChangeStatus(OrderStatus.Preparing));
    }
}
