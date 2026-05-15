using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.ValueObjects;
using FrangoFrito.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        FrangoFritoDbContext dbContext,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        CancellationToken cancellationToken = default)
    {
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        await CreateUserIfMissingAsync(userManager, "Admin Vovo", "admin@frangofrito.local", AppRoles.Admin);
        await CreateUserIfMissingAsync(userManager, "Ana Atendente", "atendente@frangofrito.local", AppRoles.Attendant);
        await CreateUserIfMissingAsync(userManager, "Carlos Cozinha", "cozinha@frangofrito.local", AppRoles.Kitchen);
        await CreateUserIfMissingAsync(userManager, "Davi Entregador", "entregador@frangofrito.local", AppRoles.Courier);

        if (await dbContext.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var combos = new Category("Combos", "Porcoes completas para dividir.");
        var frangos = new Category("Frangos", "Frango frito crocante no tempero da vovo.");
        var acompanhamentos = new Category("Acompanhamentos", "Classicos para completar o pedido.");
        var bebidas = new Category("Bebidas", "Refrigerantes e sucos.");

        dbContext.Categories.AddRange(combos, frangos, acompanhamentos, bebidas);

        dbContext.Products.AddRange(
            new Product("Balde da Vovo", "12 pedacos crocantes, batata rustica e dois molhos.", 79.90m, combos.Id, imageUrl: "/images/products/balde-da-vovo.jpg"),
            new Product("Combo Familia", "20 pedacos, duas batatas e refrigerante 2L.", 129.90m, combos.Id, imageUrl: "/images/products/combo-familia.jpg"),
            new Product("Coxa Crocante", "Porcao com 6 coxas temperadas e empanadas.", 39.90m, frangos.Id, imageUrl: "/images/products/coxa-crocante.jpg"),
            new Product("Tirinhas de Frango", "Tirinhas crocantes com molho especial.", 34.90m, frangos.Id, imageUrl: "/images/products/tirinhas.jpg"),
            new Product("Batata Rustica", "Batata com alecrim, sal grosso e maionese da casa.", 18.90m, acompanhamentos.Id, imageUrl: "/images/products/batata-rustica.jpg"),
            new Product("Polenta Frita", "Polenta sequinha com parmesao.", 16.90m, acompanhamentos.Id, imageUrl: "/images/products/polenta.jpg"),
            new Product("Refrigerante 2L", "Coca-Cola, Guarana ou laranja.", 13.90m, bebidas.Id, imageUrl: "/images/products/refrigerante.jpg"),
            new Product("Suco Natural", "Laranja, maracuja ou limao.", 9.90m, bebidas.Id, imageUrl: "/images/products/suco.jpg"));

        dbContext.Customers.AddRange(
            new Customer(
                "Maria Oliveira",
                "(11) 98888-1000",
                new Address("Rua das Palmeiras", "120", "Centro", "Sao Paulo", "SP", "01010-000", "Apto 42")),
            new Customer(
                "Joao Santos",
                "(11) 97777-2000",
                new Address("Avenida Brasil", "455", "Jardins", "Sao Paulo", "SP", "01400-000", null)));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task CreateUserIfMissingAsync(
        UserManager<AppUser> userManager,
        string fullName,
        string email,
        string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(user, "Vovo@12345");
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Nao foi possivel criar usuario seed {email}: {errors}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}
