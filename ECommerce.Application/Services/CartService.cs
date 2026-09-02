using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Models;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderService orderService,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderService = orderService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            return new CartDto
            {
                Items = new List<CartItemDto>(),
                TotalAmount = 0
            };
        }

        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(
        string userId,
        AddToCartDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product =
            await _productRepository.GetByIdAsync(
                dto.ProductId);

        if (product == null)
        {
            throw new ArgumentException(
                $"Product {dto.ProductId} was not found.");
        }

        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _cartRepository.AddAsync(cart);
            await _cartRepository.SaveChangesAsync();

            cart =
                await _cartRepository.GetByUserIdAsync(userId);

            if (cart == null)
            {
                throw new InvalidOperationException(
                    "Cart could not be created.");
            }
        }

        var existingItem =
            await _cartRepository.GetItemAsync(
                cart.Id,
                dto.ProductId);

        var newQuantity =
            existingItem == null
                ? dto.Quantity
                : existingItem.Quantity + dto.Quantity;

        if (product.StockQuantity < newQuantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for product '{product.Name}'.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = newQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = dto.Quantity
            };

            await _cartRepository.AddItemAsync(cartItem);
        }

        await _cartRepository.SaveChangesAsync();

        cart =
            await _cartRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException(
                "Cart could not be loaded.");

        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(
        string userId,
        int productId,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            throw new InvalidOperationException(
                "Cart not found.");
        }

        var item =
            await _cartRepository.GetItemAsync(
                cart.Id,
                productId);

        if (item == null)
        {
            throw new ArgumentException(
                "Product is not in the cart.");
        }

        var product =
            await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            throw new ArgumentException(
                $"Product {productId} was not found.");
        }

        if (product.StockQuantity < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for product '{product.Name}'.");
        }

        item.Quantity = quantity;

        await _cartRepository.SaveChangesAsync();

        cart =
            await _cartRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException(
                "Cart could not be loaded.");

        return MapToDto(cart);
    }

    public async Task RemoveFromCartAsync(
        string userId,
        int productId)
    {
        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            return;
        }

        var item =
            await _cartRepository.GetItemAsync(
                cart.Id,
                productId);

        if (item == null)
        {
            return;
        }

        await _cartRepository.RemoveItemAsync(item);

        await _cartRepository.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            return;
        }

        foreach (var item in cart.Items.ToList())
        {
            await _cartRepository.RemoveItemAsync(item);
        }

        await _cartRepository.SaveChangesAsync();
    }

    public async Task<OrderDto> CheckoutAsync(string userId)
    {
        // Important:
        // Do NOT load Product entities here.
        // This prevents the EF Core tracking conflict.
        var cart =
            await _cartRepository.GetForCheckoutAsync(userId);

        if (cart == null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Cart is empty.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var orderDto = new CreateOrderDto
            {
                Items = cart.Items.Select(item =>
                    new CreateOrderItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
            };

            var order =
                await _orderService.CreateOrderAsync(
                    orderDto,
                    userId);

            foreach (var item in cart.Items.ToList())
            {
                await _cartRepository.RemoveItemAsync(item);
            }

            // Save cart changes.
            // OrderService has already saved the order and
            // therefore order.Id already contains the real ID.
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return order;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(item => new CartItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            UnitPrice = item.Product?.Price ?? 0,
            Quantity = item.Quantity,
            TotalPrice =
                (item.Product?.Price ?? 0) * item.Quantity
        }).ToList();

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            TotalAmount = items.Sum(i => i.TotalPrice)
        };
    }
}