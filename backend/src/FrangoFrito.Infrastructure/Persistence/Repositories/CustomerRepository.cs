using FrangoFrito.Application.Common;
using FrangoFrito.Application.Customers;
using FrangoFrito.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly FrangoFritoDbContext _dbContext;

    public CustomerRepository(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(customer =>
                EF.Functions.ILike(customer.Name, term) ||
                EF.Functions.ILike(customer.Phone, term));
        }

        return await query
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.Name,
                customer.Phone,
                new AddressDto(
                    customer.Address.Street,
                    customer.Address.Number,
                    customer.Address.Neighborhood,
                    customer.Address.City,
                    customer.Address.State,
                    customer.Address.ZipCode,
                    customer.Address.Complement),
                customer.CreatedAt,
                customer.UpdatedAt))
            .ToPagedResultAsync(new PagedQuery(page, pageSize, search), cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Customers.AnyAsync(customer => customer.Id == id, cancellationToken);

    public Task<bool> HasOrdersAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Orders.AnyAsync(order => order.CustomerId == id, cancellationToken);

    public void Add(Customer customer) => _dbContext.Customers.Add(customer);

    public void Remove(Customer customer) => _dbContext.Customers.Remove(customer);
}
