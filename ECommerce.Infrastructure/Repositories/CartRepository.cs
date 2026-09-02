using ECommerce.Application.Interfaces;
using ECommerce.Domain.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        return await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetByIdAsync(int cartId)
    {
        return await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    public async Task<Cart?> GetForCheckoutAsync(string userId)
    {
        return await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);

        return cart;
    }

    public async Task<CartItem> AddItemAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);

        return item;
    }

    public async Task<CartItem?> GetItemAsync(
        int cartId,
        int productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(
                i => i.CartId == cartId &&
                     i.ProductId == productId);
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);

        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}