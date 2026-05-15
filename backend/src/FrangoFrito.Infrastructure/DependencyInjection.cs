using FrangoFrito.Application.Auth;
using FrangoFrito.Application.Categories;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Menu;
using FrangoFrito.Application.Orders;
using FrangoFrito.Application.Products;
using FrangoFrito.Application.Security;
using FrangoFrito.Infrastructure.Identity;
using FrangoFrito.Infrastructure.Persistence;
using FrangoFrito.Infrastructure.Persistence.Repositories;
using FrangoFrito.Infrastructure.Security;
using FrangoFrito.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrangoFrito.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=frango_frito_da_vovo;Username=postgres;Password=postgres";

        services.AddDbContext<FrangoFritoDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<FrangoFritoDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
