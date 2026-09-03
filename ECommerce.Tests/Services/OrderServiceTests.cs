using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository>
        _orderRepositoryMock;

    private readonly Mock<IProductRepository>
        _productRepositoryMock;

    private readonly Mock<IBackgroundJobQueue>
        _backgroundJobQueueMock;

    private readonly Mock<IOrderBackgroundProcessor>
        _orderBackgroundProcessorMock;

    private readonly Mock<IOrderNotificationService>
        _orderNotificationServiceMock;

    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock =
            new Mock<IOrderRepository>();

        _productRepositoryMock =
            new Mock<IProductRepository>();

        _backgroundJobQueueMock =
            new Mock<IBackgroundJobQueue>();

        _orderBackgroundProcessorMock =
            new Mock<IOrderBackgroundProcessor>();

        _orderNotificationServiceMock =
            new Mock<IOrderNotificationService>();

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _backgroundJobQueueMock.Object,
            _orderBackgroundProcessorMock.Object,
            _orderNotificationServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_OrderExistsAndBelongsToUser_ReturnsOrder()
    {
        var userId = "user-1";

        var order = new Order
        {
            Id = 1,
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100,
            Status = "Pending",
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 50,
                    Product = new Product
                    {
                        Id = 1,
                        Name = "Laptop"
                    }
                }
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var result =
            await _orderService.GetByIdAsync(1, userId);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(100, result.TotalAmount);
        Assert.Equal("Pending", result.Status);

        Assert.Single(result.Items);

        Assert.Equal(
            "Laptop",
            result.Items.First().ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_OrderDoesNotExist_ReturnsNull()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var result =
            await _orderService.GetByIdAsync(
                999,
                "user-1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_OrderBelongsToAnotherUser_ReturnsNull()
    {
        var order = new Order
        {
            Id = 1,
            UserId = "user-2",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100,
            Status = "Pending"
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var result =
            await _orderService.GetByIdAsync(
                1,
                "user-1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMyOrdersAsync_ReturnsUserOrders()
    {
        var userId = "user-1";

        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 100,
                Status = "Pending"
            },
            new Order
            {
                Id = 2,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 200,
                Status = "Paid"
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(orders);

        var result =
            await _orderService.GetMyOrdersAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetMyOrdersAsync_NoOrders_ReturnsEmptyCollection()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByUserIdAsync("user-1"))
            .ReturnsAsync(new List<Order>());

        var result =
            await _orderService.GetMyOrdersAsync("user-1");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyItems_ThrowsArgumentException()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>()
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(
                dto,
                "user-1"));
    }

    [Fact]
    public async Task CreateOrderAsync_NullItems_ThrowsArgumentException()
    {
        var dto = new CreateOrderDto
        {
            Items = null!
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(
                dto,
                "user-1"));
    }

    [Fact]
    public async Task CreateOrderAsync_InvalidQuantity_ThrowsArgumentException()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    Quantity = 0
                }
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(
                dto,
                "user-1"));
    }

    [Fact]
    public async Task CreateOrderAsync_ProductDoesNotExist_ThrowsArgumentException()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 999,
                    Quantity = 1
                }
            }
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.CreateOrderAsync(
                dto,
                "user-1"));
    }

    [Fact]
    public async Task CreateOrderAsync_InsufficientStock_ThrowsInvalidOperationException()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 500,
            StockQuantity = 2
        };

        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    Quantity = 5
                }
            }
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orderService.CreateOrderAsync(
                dto,
                "user-1"));
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_CreatesOrderAndUpdatesStock()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 500,
            StockQuantity = 10
        };

        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                order.Id = 10;
            })
            .ReturnsAsync(
                (Order order) => order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(r => r.UpdateAsync(
                It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _backgroundJobQueueMock
            .Setup(q => q.QueueAsync(
                It.IsAny<Func<CancellationToken, Task>>()))
            .Returns(
                new ValueTask());

        var result =
            await _orderService.CreateOrderAsync(
                dto,
                "user-1");

        Assert.NotNull(result);

        Assert.Equal(10, result.Id);

        Assert.Equal(
            "user-1",
            result.UserId);

        Assert.Equal(
            1000,
            result.TotalAmount);

        Assert.Equal(
            "Pending",
            result.Status);

        Assert.Single(result.Items);

        Assert.Equal(
            1,
            result.Items.First().ProductId);

        Assert.Equal(
            2,
            result.Items.First().Quantity);

        Assert.Equal(
            500,
            result.Items.First().UnitPrice);

        Assert.Equal(
            1000,
            result.Items.First().TotalPrice);

        Assert.Equal(
            8,
            product.StockQuantity);

        _productRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Product>(
                    p => p.Id == 1 &&
                         p.StockQuantity == 8)),
            Times.Once);

        _orderRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Order>(
                    o => o.UserId == "user-1" &&
                         o.TotalAmount == 1000 &&
                         o.Status == "Pending")),
            Times.Once);

        _orderRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        _backgroundJobQueueMock.Verify(
            q => q.QueueAsync(
                It.IsAny<Func<CancellationToken, Task>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_MultipleItems_CalculatesTotalCorrectly()
    {
        var product1 = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 500,
            StockQuantity = 10
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Mouse",
            Price = 50,
            StockQuantity = 20
        };

        var dto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    Quantity = 2
                },
                new CreateOrderItemDto
                {
                    ProductId = 2,
                    Quantity = 3
                }
            }
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product1);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(product2);

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                order.Id = 20;
            })
            .ReturnsAsync(
                (Order order) => order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _productRepositoryMock
            .Setup(r => r.UpdateAsync(
                It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _backgroundJobQueueMock
            .Setup(q => q.QueueAsync(
                It.IsAny<Func<CancellationToken, Task>>()))
            .Returns(
                new ValueTask());

        var result =
            await _orderService.CreateOrderAsync(
                dto,
                "user-1");

        Assert.Equal(
            1150,
            result.TotalAmount);

        Assert.Equal(
            8,
            product1.StockQuantity);

        Assert.Equal(
            17,
            product2.StockQuantity);

        Assert.Equal(
            2,
            result.Items.Count);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                UserId = "user-1",
                TotalAmount = 100,
                Status = "Pending",
                OrderDate = DateTime.UtcNow
            },
            new Order
            {
                Id = 2,
                UserId = "user-2",
                TotalAmount = 200,
                Status = "Paid",
                OrderDate = DateTime.UtcNow
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        var result =
            await _orderService.GetAllOrdersAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdForAdminAsync_OrderExists_ReturnsOrder()
    {
        var order = new Order
        {
            Id = 1,
            UserId = "user-1",
            TotalAmount = 100,
            Status = "Pending",
            OrderDate = DateTime.UtcNow
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var result =
            await _orderService.GetByIdForAdminAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdForAdminAsync_OrderDoesNotExist_ReturnsNull()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var result =
            await _orderService.GetByIdForAdminAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_EmptyStatus_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.UpdateStatusAsync(
                1,
                ""));
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidStatus_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _orderService.UpdateStatusAsync(
                1,
                "InvalidStatus"));
    }

    [Fact]
    public async Task UpdateStatusAsync_OrderDoesNotExist_ReturnsNull()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var result =
            await _orderService.UpdateStatusAsync(
                999,
                "Paid");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_UpdatesOrder()
    {
        var order = new Order
        {
            Id = 1,
            UserId = "user-1",
            TotalAmount = 100,
            Status = "Pending",
            OrderDate = DateTime.UtcNow
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _orderNotificationServiceMock
            .Setup(n => n.NotifyOrderStatusChangedAsync(
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result =
            await _orderService.UpdateStatusAsync(
                1,
                "Paid");

        Assert.NotNull(result);
        Assert.Equal("Paid", result.Status);
        Assert.Equal("Paid", order.Status);

        _orderRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        _orderNotificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(
                1,
                "Paid"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_StatusIsCaseInsensitive_NormalizesStatus()
    {
        var order = new Order
        {
            Id = 1,
            UserId = "user-1",
            TotalAmount = 100,
            Status = "Pending",
            OrderDate = DateTime.UtcNow
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _orderNotificationServiceMock
            .Setup(n => n.NotifyOrderStatusChangedAsync(
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result =
            await _orderService.UpdateStatusAsync(
                1,
                "sHiPpEd");

        Assert.NotNull(result);
        Assert.Equal("Shipped", result.Status);
        Assert.Equal("Shipped", order.Status);

        _orderNotificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(
                1,
                "Shipped"),
            Times.Once);
    }
}