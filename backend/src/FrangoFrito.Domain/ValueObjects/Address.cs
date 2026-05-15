using FrangoFrito.Domain.Common;

namespace FrangoFrito.Domain.ValueObjects;

public sealed class Address
{
    private Address()
    {
    }

    public Address(
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string? complement)
    {
        Street = Required(street, "Rua");
        Number = Required(number, "Numero");
        Neighborhood = Required(neighborhood, "Bairro");
        City = Required(city, "Cidade");
        State = Required(state, "Estado").ToUpperInvariant();
        ZipCode = Required(zipCode, "CEP");
        Complement = complement?.Trim() ?? string.Empty;
    }

    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Complement { get; private set; } = string.Empty;

    public override string ToString()
    {
        var complement = string.IsNullOrWhiteSpace(Complement) ? string.Empty : $" - {Complement}";
        return $"{Street}, {Number}{complement} - {Neighborhood}, {City}/{State} - {ZipCode}";
    }

    private static string Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{field} e obrigatorio.");
        }

        return value.Trim();
    }
}
