using FrangoFrito.Domain.Common;

namespace FrangoFrito.Domain.Entities;

public sealed class Category : Entity
{
    private Category()
    {
    }

    public Category(string name, string description)
    {
        Rename(name);
        UpdateDescription(description);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public void Update(string name, string description)
    {
        Rename(name);
        UpdateDescription(description);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nome da categoria e obrigatorio.");
        }

        Name = name.Trim();
    }

    private void UpdateDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }
}
