using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Orders;
using FrangoFrito.Application.Products;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.ValueObjects;

namespace FrangoFrito.Application.Common;

public static class DtoMappingExtensions
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Description, category.IsActive, category.CreatedAt, category.UpdatedAt);

    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsActive,
            product.ImageUrl,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.CreatedAt,
            product.UpdatedAt);

    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.Name, customer.Phone, customer.Address.ToDto(), customer.CreatedAt, customer.UpdatedAt);

    public static AddressDto ToDto(this Address address) =>
        new(address.Street, address.Number, address.Neighborhood, address.City, address.State, address.ZipCode, address.Complement);

    public static Address ToDomain(this AddressDto address) =>
        new(address.Street, address.Number, address.Neighborhood, address.City, address.State, address.ZipCode, address.Complement);

    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Customer?.Name ?? string.Empty,
            order.Status.ToString(),
            order.Total,
            order.TotalItems,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(item => new OrderItemDto(item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.Total)).ToArray());
}
