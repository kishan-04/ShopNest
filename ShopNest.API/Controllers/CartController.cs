using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.API.DTOs;
using ShopNest.API.Services;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // All cart endpoints require login
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    // Helper to get current logged in user's ID from JWT token
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // GET api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(cart);
    }

    // POST api/cart
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        try
        {
            await _cartService.AddToCartAsync(GetUserId(), dto);
            return Ok(new { message = "Item added to cart." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT api/cart/{productId}?quantity=3
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateItem(int productId, [FromQuery] int quantity)
    {
        try
        {
            await _cartService.UpdateItemAsync(GetUserId(), productId, quantity);
            return Ok(new { message = "Cart updated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE api/cart/{productId}
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        try
        {
            await _cartService.RemoveItemAsync(GetUserId(), productId);
            return Ok(new { message = "Item removed from cart." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE api/cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            await _cartService.ClearCartAsync(GetUserId());
            return Ok(new { message = "Cart cleared." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}