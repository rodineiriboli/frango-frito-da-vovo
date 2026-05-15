using FrangoFrito.Infrastructure.Identity;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FrangoFrito.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<FrangoFritoDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FrangoFritoDbContext>>();

            var databaseName = $"frango-frito-tests-{Guid.NewGuid()}";
            services.AddDbContext<FrangoFritoDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<FrangoFritoDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            DatabaseSeeder.SeedAsync(dbContext, userManager, roleManager).GetAwaiter().GetResult();
        });
    }
}
