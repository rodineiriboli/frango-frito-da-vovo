using FrangoFrito.Application.Common;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Customers;

public interface ICustomerRepository
{
    Task<PagedResult<CustomerDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasOrdersAsync(Guid id, CancellationToken cancellationToken);
    void Add(Customer customer);
    void Remove(Customer customer);
}
