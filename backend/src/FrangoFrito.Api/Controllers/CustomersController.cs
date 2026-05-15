using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Attendant)]
public sealed class CustomersController : ControllerBase
{
    private readonly FrangoFritoDbContext _dbContext;

    public CustomersController(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(customer =>
                EF.Functions.ILike(customer.Name, term) ||
                EF.Functions.ILike(customer.Phone, term));
        }

        var result = await query
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

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return customer is null ? NotFound() : Ok(customer.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = new Customer(request.Name, request.Phone, request.Address.ToDomain());

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        customer.Update(request.Name, request.Phone, request.Address.ToDomain());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(customer.ToDto());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        var hasOrders = await _dbContext.Orders.AnyAsync(order => order.CustomerId == id, cancellationToken);
        if (hasOrders)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cliente possui pedidos.",
                Detail = "Clientes com historico de pedidos nao podem ser removidos.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
