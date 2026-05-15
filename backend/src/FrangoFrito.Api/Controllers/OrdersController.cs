using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Orders;
using FrangoFrito.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetAllAsync(page, pageSize, status, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OrderCreation)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateAsync(request, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateStatusAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : result.ToActionResult(this);
    }
}
