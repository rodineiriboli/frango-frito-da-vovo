namespace FrangoFrito.IntegrationTests;

public sealed class MenuApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MenuApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMenu_ShouldReturnSeededProducts()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/menu");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Balde da Vovó", body);
    }
}
