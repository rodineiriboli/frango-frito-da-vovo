using FrangoFrito.Api.Extensions;
using FrangoFrito.Application.Common;
using FrangoFrito.Application.Orders;
using FrangoFrito.Application.Security;
using FrangoFrito.Domain.Entities;
using FrangoFrito.Domain.Enums;
using FrangoFrito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrangoFrito.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = AppPolicies.Backoffice)]
public sealed class OrdersController : ControllerBase
{
    private readonly FrangoFritoDbContext _dbContext;

    public OrdersController(FrangoFritoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .AsQueryable();

        query = ApplyRoleScope(query);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                return this.ValidationErrors(new Dictionary<string, string[]> { ["status"] = ["Status invalido."] });
            }

            query = query.Where(order => order.Status == parsedStatus);
        }

        var paging = new PagedQuery(page, pageSize);
        var totalItems = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.CreatedAt)
            .Skip((paging.SafePage - 1) * paging.SafePageSize)
            .Take(paging.SafePageSize)
            .ToArrayAsync(cancellationToken);

        return Ok(new PagedResult<OrderDto>(
            orders.Select(order => order.ToDto()).ToArray(),
            paging.SafePage,
            paging.SafePageSize,
            totalItems));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return order is null ? NotFound() : Ok(order.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OrderCreation)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Customers.AnyAsync(customer => customer.Id == request.CustomerId, cancellationToken))
        {
            return this.ValidationErrors(new Dictionary<string, string[]> { ["customerId"] = ["Cliente inexistente."] });
        }

        var requestedProducts = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
            .ToArray();

        var productIds = requestedProducts.Select(item => item.ProductId).ToArray();
        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var order = new Order(request.CustomerId);
        foreach (var requestedProduct in requestedProducts)
        {
            if (!products.TryGetValue(requestedProduct.ProductId, out var product))
            {
                return this.ValidationErrors(new Dictionary<string, string[]> { ["items"] = [$"Produto {requestedProduct.ProductId} inexistente."] });
            }

            order.AddItem(product, requestedProduct.Quantity);
        }

        order.EnsureHasItems();

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Items)
            .FirstAsync(item => item.Id == order.Id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, created.ToDto());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            return this.ValidationErrors(new Dictionary<string, string[]> { ["status"] = ["Status invalido."] });
        }

        if (!CanChangeTo(newStatus))
        {
            return Forbid();
        }

        var order = await _dbContext.Orders
            .Include(item => item.Customer)
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        order.ChangeStatus(newStatus);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(order.ToDto());
    }

    private IQueryable<Order> ApplyRoleScope(IQueryable<Order> query)
    {
        if (User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Attendant))
        {
            return query;
        }

        if (User.IsInRole(AppRoles.Kitchen))
        {
            return query.Where(order => order.Status == OrderStatus.Received || order.Status == OrderStatus.Preparing);
        }

        if (User.IsInRole(AppRoles.Courier))
        {
            return query.Where(order => order.Status == OrderStatus.OutForDelivery || order.Status == OrderStatus.Delivered);
        }

        return query.Where(order => false);
    }

    private bool CanChangeTo(OrderStatus targetStatus)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        if (User.IsInRole(AppRoles.Attendant))
        {
            return targetStatus == OrderStatus.Cancelled;
        }

        if (User.IsInRole(AppRoles.Kitchen))
        {
            return targetStatus == OrderStatus.Preparing;
        }

        if (User.IsInRole(AppRoles.Courier))
        {
            return targetStatus is OrderStatus.OutForDelivery or OrderStatus.Delivered;
        }

        return false;
    }
}
