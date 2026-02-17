using Comanda.Api.Hubs;
using Comanda.Core.DTOs;
using Comanda.Core.Entities;
using Comanda.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Comanda.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ComandaDbContext _context;
    private readonly IHubContext<OrdersHub> _hubContext;

    public OrdersController(ComandaDbContext context, IHubContext<OrdersHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetOrders(
        [FromQuery] Guid? userId,
        [FromQuery] string? role,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedByUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var roleValid = Enum.TryParse<UserRole>(role, ignoreCase: true, out var r);
            if (roleValid)
            {
                if (r == UserRole.Waiter && userId.HasValue)
                    query = query.Where(o => o.CreatedByUserId == userId.Value);
                else if (r == UserRole.Cook)
                    query = query.Where(o => o.Status != OrderStatus.Ready);
            }
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var st))
            query = query.Where(o => o.Status == st);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return Ok(orders.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromQuery] Guid createdByUserId,
        CancellationToken ct)
    {
        var user = await _context.Users.FindAsync([createdByUserId], ct);
        if (user == null)
            return BadRequest("User not found");

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            TableNumber = request.TableNumber,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductName = i.ProductName,
                Quantity = i.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.Group("cooks").SendAsync("OrderCreated", MapToResponse(order));

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapToResponse(order));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id, CancellationToken ct)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedByUser)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order == null)
            return NotFound();

        return Ok(MapToResponse(order));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        var statusValid = Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status);
        if (!statusValid)
            return BadRequest("Invalid status. Must be Pending, Preparing, or Ready.");

        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.CreatedByUser)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order == null)
            return NotFound();

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients.Group("waiters").SendAsync("OrderStatusChanged", new
        {
            orderId = order.Id,
            status = order.Status.ToString()
        });

        return Ok(MapToResponse(order));
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.TableNumber,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.CreatedByUserId,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductName, i.Quantity)).ToList()
        );
    }
}
