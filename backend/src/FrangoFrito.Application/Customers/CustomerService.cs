using FrangoFrito.Application.Common;
using FrangoFrito.Application.Common.Abstractions;
using FrangoFrito.Domain.Entities;

namespace FrangoFrito.Application.Customers;

internal sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<CustomerDto>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken) =>
        _customerRepository.GetPagedAsync(page, pageSize, search, cancellationToken);

    public async Task<ApplicationResult<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null
            ? ApplicationResult<CustomerDto>.NotFound()
            : ApplicationResult<CustomerDto>.Success(customer.ToDto());
    }

    public async Task<ApplicationResult<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = new Customer(request.Name, request.Phone, request.Address.ToDomain());

        _customerRepository.Add(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApplicationResult<CustomerDto>.Success(customer.ToDto());
    }

    public async Task<ApplicationResult<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return ApplicationResult<CustomerDto>.NotFound();
        }

        customer.Update(request.Name, request.Phone, request.Address.ToDomain());
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApplicationResult<CustomerDto>.Success(customer.ToDto());
    }

    public async Task<ApplicationResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return ApplicationResult.NotFound();
        }

        if (await _customerRepository.HasOrdersAsync(id, cancellationToken))
        {
            return ApplicationResult.Conflict(
                "Cliente possui pedidos.",
                "Clientes com histórico de pedidos não podem ser removidos.");
        }

        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success();
    }
}
