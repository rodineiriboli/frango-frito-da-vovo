namespace FrangoFrito.Application.Customers;

public sealed record AddressDto(
    string Street,
    string Number,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Complement);

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string Phone,
    AddressDto Address,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateCustomerRequest(
    string Name,
    string Phone,
    AddressDto Address);

public sealed record UpdateCustomerRequest(
    string Name,
    string Phone,
    AddressDto Address);
