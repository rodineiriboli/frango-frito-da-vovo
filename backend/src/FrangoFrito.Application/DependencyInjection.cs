using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Menu;
using FrangoFrito.Application.Orders;
using FrangoFrito.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace FrangoFrito.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMenuService, MenuService>();

        return services;
    }
}
