using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // =========================================================
    // CUSTOMER ENDPOINTS
    // =========================================================

    // GET: api/Orders
    // Customer can see only their own orders.
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var orders =
            await _orderService.GetMyOrdersAsync(userId);

        return Ok(orders);
    }

    // GET: api/Orders/5
    // Customer can see only their own order.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var order =
            await _orderService.GetByIdAsync(
                id,
                userId);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    // POST: api/Orders
    // Customer creates an order.
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderDto dto)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var order =
                await _orderService.CreateOrderAsync(
                    dto,
                    userId);

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id },
                order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // =========================================================
    // ADMIN ENDPOINTS
    // =========================================================

    // GET: api/Orders/admin
    // Admin can see ALL orders.
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders =
            await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    // GET: api/Orders/admin/5
    // Admin can see any order.
    [HttpGet("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrderForAdmin(int id)
    {
        var order =
            await _orderService.GetByIdForAdminAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    // PUT: api/Orders/admin/5/status
    // Admin can update order status.
    [HttpPut("admin/{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        UpdateOrderStatusDto dto)
    {
        try
        {
            var order =
                await _orderService.UpdateStatusAsync(
                    id,
                    dto.Status);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}