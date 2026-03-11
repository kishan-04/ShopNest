using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.API.DTOs;
using ShopNest.API.Services;

namespace ShopNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // POST api/orders — place an order
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
    {
        try
        {
            var orderId = await _orderService.PlaceOrderAsync(GetUserId(), dto);
            return Ok(new { orderId, message = "Order placed successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/orders/my — get current user's orders
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _orderService.GetMyOrdersAsync(GetUserId());
        return Ok(orders);
    }

    // GET api/orders — get all orders (Admin only)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    // PUT api/orders/{id}/status — update order status (Admin only)
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var updated = await _orderService.UpdateStatusAsync(id, dto.Status);
            if (!updated)
                return NotFound(new { message = "Order not found." });
            return Ok(new { message = "Order status updated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/orders/by-user — Admin only
    [HttpGet("by-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrdersByUser()
    {
        var result = await _orderService.GetOrdersByUserAsync();
        return Ok(result);
    }
}