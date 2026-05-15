using FrangoFrito.Application.Common;

namespace FrangoFrito.Application.Customers;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken);
    Task<ApplicationResult<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationResult<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken);
    Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
