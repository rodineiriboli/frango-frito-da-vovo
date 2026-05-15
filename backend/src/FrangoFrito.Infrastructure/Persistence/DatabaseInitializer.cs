using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FrangoFrito.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<FrangoFritoDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (dbContext.Database.IsRelational())
        {
            var migrations = dbContext.Database.GetMigrations();
            if (migrations.Any())
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await DatabaseSeeder.SeedAsync(dbContext, userManager, roleManager, cancellationToken);
    }
}
