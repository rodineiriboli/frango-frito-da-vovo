using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Customers;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Attendant)]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerService.GetAllAsync(page, pageSize, search, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }
}
