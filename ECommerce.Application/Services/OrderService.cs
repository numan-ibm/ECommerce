using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // =========================================================
    // CUSTOMER OPERATIONS
    // =========================================================

    public async Task<OrderDto?> GetByIdAsync(
        int id,
        string userId)
    {
        var order =
            await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            return null;
        }

        // Customer can only view their own order
        if (order.UserId != userId)
        {
            return null;
        }

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(
        string userId)
    {
        var orders =
            await _orderRepository.GetByUserIdAsync(userId);

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(
        CreateOrderDto dto,
        string userId)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ArgumentException(
                "Order must contain at least one item.");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        decimal totalAmount = 0;

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var product =
                await _productRepository.GetByIdAsync(
                    itemDto.ProductId);

            if (product == null)
            {
                throw new ArgumentException(
                    $"Product {itemDto.ProductId} was not found.");
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'.");
            }

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            };

            order.OrderItems.Add(orderItem);

            totalAmount +=
                product.Price * itemDto.Quantity;

            // Reduce inventory
            product.StockQuantity -= itemDto.Quantity;

            await _productRepository.UpdateAsync(product);
        }

        order.TotalAmount = totalAmount;

        await _orderRepository.AddAsync(order);

        // Save so EF Core generates the database Order ID.
        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    // =========================================================
    // ADMIN OPERATIONS
    // =========================================================

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders =
            await _orderRepository.GetAllAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetByIdForAdminAsync(
        int id)
    {
        var order =
            await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            return null;
        }

        return MapToDto(order);
    }

    public async Task<OrderDto?> UpdateStatusAsync(
        int id,
        string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException(
                "Order status is required.");
        }

        var allowedStatuses = new[]
        {
            "Pending",
            "Paid",
            "Shipped",
            "Delivered",
            "Cancelled"
        };

        var normalizedStatus =
            status.Trim();

        var validStatus =
            allowedStatuses.FirstOrDefault(
                s => s.Equals(
                    normalizedStatus,
                    StringComparison.OrdinalIgnoreCase));

        if (validStatus == null)
        {
            throw new ArgumentException(
                "Invalid order status. " +
                "Allowed statuses: Pending, Paid, Shipped, Delivered, Cancelled.");
        }

        var order =
            await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            return null;
        }

        order.Status = validStatus;

        await _orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    // =========================================================
    // MAPPING
    // =========================================================

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,

            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice =
                    item.UnitPrice * item.Quantity
            }).ToList()
        };
    }
}