using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrangoFrito.IntegrationTests;

public sealed class CategoriesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CategoriesApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteCategory_ShouldRejectCategoryWithProducts()
    {
        var client = _factory.CreateClient();
        await LoginAsAdminAsync(client);

        var categories = await client.GetStringAsync("/api/categories");
        using var document = JsonDocument.Parse(categories);
        var usedCategoryId = document.RootElement[0].GetProperty("id").GetString();

        var response = await client.DeleteAsync($"/api/categories/{usedCategoryId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Categorias vinculadas a produtos não podem ser excluídas.", body);
    }

    [Fact]
    public async Task DeleteCategory_ShouldRemoveCategoryWithoutProducts()
    {
        var client = _factory.CreateClient();
        await LoginAsAdminAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/categories", new
        {
            name = $"Temporaria {Guid.NewGuid():N}",
            description = "Categoria sem produtos"
        });
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(created);
        var categoryId = document.RootElement.GetProperty("id").GetString();

        var deleteResponse = await client.DeleteAsync($"/api/categories/{categoryId}");
        var getResponse = await client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static async Task LoginAsAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@frangofrito.local",
            password = "Vovo@12345",
            rememberMe = true
        });
        response.EnsureSuccessStatusCode();

        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(value => value.Split(';', 2)[0]);
        client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));
    }
}
