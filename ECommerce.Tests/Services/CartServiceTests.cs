using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;
using Moq;

namespace ECommerce.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _cartRepositoryMock =
            new Mock<ICartRepository>();

        _productRepositoryMock =
            new Mock<IProductRepository>();

        _orderServiceMock =
            new Mock<IOrderService>();

        _unitOfWorkMock =
            new Mock<IUnitOfWork>();

        _cartService =
            new CartService(
                _cartRepositoryMock.Object,
                _productRepositoryMock.Object,
                _orderServiceMock.Object,
                _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetCartAsync_CartDoesNotExist_ReturnsEmptyCart()
    {
        // Arrange
        _cartRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync("user-1"))
            .ReturnsAsync((Cart?)null);

        // Act
        var result =
            await _cartService.GetCartAsync("user-1");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalAmount);
    }

    [Fact]
    public async Task GetCartAsync_CartExists_ReturnsCart()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            StockQuantity = 10
        };

        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1",
            Items = new List<CartItem>
            {
                new CartItem
                {
                    CartId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    Product = product
                }
            }
        };

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync("user-1"))
            .ReturnsAsync(cart);

        // Act
        var result =
            await _cartService.GetCartAsync("user-1");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Laptop", result.Items[0].ProductName);
        Assert.Equal(999.99m, result.Items[0].UnitPrice);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(1999.98m, result.TotalAmount);
    }

    [Fact]
    public async Task AddToCartAsync_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 0
        };

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.AddToCartAsync(
                    "user-1",
                    dto));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ProductDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            ProductId = 999,
            Quantity = 1
        };

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.AddToCartAsync(
                    "user-1",
                    dto));

        Assert.Equal(
            "Product 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_InsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            StockQuantity = 2
        };

        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1"
        };

        var dto = new AddToCartDto
        {
            ProductId = 1,
            Quantity = 3
        };

        _productRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(1))
            .ReturnsAsync(product);

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync("user-1"))
            .ReturnsAsync(cart);

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetItemAsync(1, 1))
            .ReturnsAsync((CartItem?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(
                    "user-1",
                    dto));

        Assert.Equal(
            "Insufficient stock for product 'Laptop'.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_InvalidQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.UpdateCartItemAsync(
                    "user-1",
                    1,
                    0));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        _cartRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync("user-1"))
            .ReturnsAsync((Cart?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.UpdateCartItemAsync(
                    "user-1",
                    1,
                    2));

        Assert.Equal(
            "Cart not found.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_ItemDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1"
        };

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetByUserIdAsync("user-1"))
            .ReturnsAsync(cart);

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetItemAsync(1, 999))
            .ReturnsAsync((CartItem?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.UpdateCartItemAsync(
                    "user-1",
                    999,
                    2));

        Assert.Equal(
            "Product is not in the cart.",
            exception.Message);
    }

    [Fact]
    public async Task CheckoutAsync_EmptyCart_ThrowsInvalidOperationException()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1",
            Items = new List<CartItem>()
        };

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetForCheckoutAsync("user-1"))
            .ReturnsAsync(cart);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.CheckoutAsync("user-1"));

        Assert.Equal(
            "Cart is empty.",
            exception.Message);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ValidCart_CreatesOrderAndCommitsTransaction()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1",
            Items = new List<CartItem>
            {
                new CartItem
                {
                    CartId = 1,
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };

        var order = new OrderDto
        {
            Id = 10,
            UserId = "user-1",
            Status = "Pending",
            TotalAmount = 1999.98m
        };

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetForCheckoutAsync("user-1"))
            .ReturnsAsync(cart);

        _orderServiceMock
            .Setup(service =>
                service.CreateOrderAsync(
                    It.IsAny<CreateOrderDto>(),
                    "user-1"))
            .ReturnsAsync(order);

        // Act
        var result =
            await _cartService.CheckoutAsync("user-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(
            "user-1",
            result.UserId);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.BeginTransactionAsync(),
            Times.Once);

        _orderServiceMock.Verify(
            service =>
                service.CreateOrderAsync(
                    It.Is<CreateOrderDto>(
                        dto =>
                            dto.Items.Count == 1 &&
                            dto.Items[0].ProductId == 1 &&
                            dto.Items[0].Quantity == 2),
                    "user-1"),
            Times.Once);

        _cartRepositoryMock.Verify(
            repository =>
                repository.RemoveItemAsync(
                    It.Is<CartItem>(
                        item =>
                            item.ProductId == 1)),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.SaveChangesAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.CommitTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.RollbackTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_OrderCreationFails_RollsBackTransaction()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 1,
            UserId = "user-1",
            Items = new List<CartItem>
            {
                new CartItem
                {
                    CartId = 1,
                    ProductId = 1,
                    Quantity = 1
                }
            }
        };

        _cartRepositoryMock
            .Setup(repository =>
                repository.GetForCheckoutAsync("user-1"))
            .ReturnsAsync(cart);

        _orderServiceMock
            .Setup(service =>
                service.CreateOrderAsync(
                    It.IsAny<CreateOrderDto>(),
                    "user-1"))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Order creation failed."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _cartService.CheckoutAsync("user-1"));

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.BeginTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.RollbackTransactionAsync(),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.CommitTransactionAsync(),
            Times.Never);
    }
}