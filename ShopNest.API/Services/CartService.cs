using System.Security.Claims;
using ShopNest.API.DTOs;
using ShopNest.API.Repositories;

namespace ShopNest.API.Services;

public class CartService
{
    private readonly CartRepository _cartRepository;
    private readonly ProductRepository _productRepository;

    public CartService(
        CartRepository cartRepository,
        ProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartDto?> GetCartAsync(int userId)
    {
        return await _cartRepository.GetCartByUserIdAsync(userId);
    }

    public async Task AddToCartAsync(int userId, AddToCartDto dto)
    {
        // Validate quantity
        if (dto.Quantity <= 0)
            throw new Exception("Quantity must be at least 1.");

        // Check product exists and has enough stock
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new Exception("Product not found.");

        if (product.Stock < dto.Quantity)
            throw new Exception($"Only {product.Stock} items left in stock.");

        // Get or create cart
        var cartId = await _cartRepository.GetOrCreateCartAsync(userId);

        // Add item with current product price
        await _cartRepository.AddItemAsync(cartId, dto, product.Price);
    }

    public async Task UpdateItemAsync(int userId, int productId, int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be at least 1.");

        // Check stock
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new Exception("Product not found.");

        if (product.Stock < quantity)
            throw new Exception($"Only {product.Stock} items left in stock.");

        var cartId = await _cartRepository.GetOrCreateCartAsync(userId);
        await _cartRepository.UpdateItemAsync(cartId, productId, quantity);
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var cartId = await _cartRepository.GetOrCreateCartAsync(userId);
        await _cartRepository.RemoveItemAsync(cartId, productId);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cartId = await _cartRepository.GetOrCreateCartAsync(userId);
        await _cartRepository.ClearCartAsync(cartId);
    }
}