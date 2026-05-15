using FrangoFrito.Domain.Common;

namespace FrangoFrito.Domain.Entities;

public sealed class Product : Entity
{
    private Product()
    {
    }

    public Product(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        bool isActive = true,
        string? imageUrl = null)
    {
        Rename(name);
        UpdateDescription(description);
        ChangePrice(price);
        CategoryId = categoryId == Guid.Empty ? throw new DomainException("Categoria é obrigatória.") : categoryId;
        ImageUrl = imageUrl?.Trim() ?? string.Empty;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public void Update(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        bool isActive,
        string? imageUrl)
    {
        Rename(name);
        UpdateDescription(description);
        ChangePrice(price);
        CategoryId = categoryId == Guid.Empty ? throw new DomainException("Categoria é obrigatória.") : categoryId;
        IsActive = isActive;
        ImageUrl = imageUrl?.Trim() ?? string.Empty;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nome do produto é obrigatório.");
        }

        Name = name.Trim();
    }

    private void UpdateDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    private void ChangePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new DomainException("Preço do produto deve ser maior que zero.");
        }

        Price = Math.Round(price, 2);
    }
}
