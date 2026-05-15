using FrangoFrito.Domain.Common;
using FrangoFrito.Domain.ValueObjects;

namespace FrangoFrito.Domain.Entities;

public sealed class Customer : Entity
{
    private Customer()
    {
    }

    public Customer(string name, string phone, Address address)
    {
        Rename(name);
        ChangePhone(phone);
        ChangeAddress(address);
    }

    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    public void Update(string name, string phone, Address address)
    {
        Rename(name);
        ChangePhone(phone);
        ChangeAddress(address);
    }

    private void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nome do cliente é obrigatório.");
        }

        Name = name.Trim();
    }

    private void ChangePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new DomainException("Telefone do cliente é obrigatório.");
        }

        Phone = phone.Trim();
    }

    private void ChangeAddress(Address address)
    {
        Address = address ?? throw new DomainException("Endereço do cliente é obrigatório.");
    }
}
