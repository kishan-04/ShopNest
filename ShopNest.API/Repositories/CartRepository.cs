using Dapper;
using ShopNest.API.Data;
using ShopNest.API.DTOs;
using ShopNest.API.Models;

namespace ShopNest.API.Repositories;

public class CartRepository
{
    private readonly DapperContext _context;

    public CartRepository(DapperContext context)
    {
        _context = context;
    }

    // Get or create cart for user
    public async Task<int> GetOrCreateCartAsync(int userId)
    {
        using var connection = _context.CreateConnection();

        // Check if cart exists
        var cartId = await connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT Id FROM Carts WHERE UserId = @UserId",
            new { UserId = userId });

        // If no cart exists, create one
        if (cartId == null)
        {
            cartId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Carts (UserId)
                  OUTPUT INSERTED.Id
                  VALUES (@UserId)",
                new { UserId = userId });
        }

        return cartId.Value;
    }

    // Get full cart with all items
    public async Task<CartDto?> GetCartByUserIdAsync(int userId)
    {
        var sql = @"
            SELECT
                c.Id,
                ci.Id,
                ci.ProductId,
                p.Name  AS ProductName,
                p.ImageUrl,
                ci.Quantity,
                ci.UnitPrice,
                p.Stock
            FROM Carts c
            LEFT JOIN CartItems ci  ON ci.CartId    = c.Id
            LEFT JOIN Products  p   ON p.Id         = ci.ProductId
            WHERE c.UserId = @UserId";

        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<CartDto, CartItemDto, CartDto>(
            sql,
            (cart, item) =>
            {
                cart.Items = new List<CartItemDto>();
                if (item != null && item.ProductId != 0)
                    cart.Items.Add(item);
                return cart;
            },
            new { UserId = userId },
            splitOn: "Id"
        );

        // Merge all rows into one cart
        var cart = result.GroupBy(c => c.Id).Select(g =>
        {
            var groupedCart = g.First();
            groupedCart.Items = g
                .Where(c => c.Items.Any())
                .SelectMany(c => c.Items)
                .ToList();
            return groupedCart;
        }).FirstOrDefault();

        return cart;
    }

    // Add item to cart
    public async Task AddItemAsync(int cartId, AddToCartDto dto, decimal unitPrice)
    {
        using var connection = _context.CreateConnection();

        // Check if product already in cart
        var existingItem = await connection.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId",
            new { CartId = cartId, ProductId = dto.ProductId });

        if (existingItem != null)
        {
            // Update quantity
            await connection.ExecuteAsync(
                @"UPDATE CartItems
                  SET Quantity = Quantity + @Quantity
                  WHERE CartId = @CartId AND ProductId = @ProductId",
                new { CartId = cartId, dto.ProductId, dto.Quantity });
        }
        else
        {
            // Insert new item
            await connection.ExecuteAsync(
                @"INSERT INTO CartItems (CartId, ProductId, Quantity, UnitPrice)
                  VALUES (@CartId, @ProductId, @Quantity, @UnitPrice)",
                new { CartId = cartId, dto.ProductId, dto.Quantity, UnitPrice = unitPrice });
        }
    }
 
    // Update item quantity
    public async Task UpdateItemAsync(int cartId, int productId, int quantity)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            @"UPDATE CartItems
              SET Quantity = @Quantity
              WHERE CartId = @CartId AND ProductId = @ProductId",
            new { CartId = cartId, ProductId = productId, Quantity = quantity });
    }

    // Remove item from cart
    public async Task RemoveItemAsync(int cartId, int productId)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId",
            new { CartId = cartId, ProductId = productId });
    }

    // Clear entire cart
    public async Task ClearCartAsync(int cartId)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM CartItems WHERE CartId = @CartId",
            new { CartId = cartId });
    }
}