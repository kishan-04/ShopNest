using ShopNest.API.DTOs;
using ShopNest.API.Repositories;

namespace ShopNest.API.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly CartRepository _cartRepository;
    private readonly EmailService _emailService;
    private readonly UserRepository _userRepository;

    public OrderService(
        OrderRepository orderRepository,
        CartRepository cartRepository,
        EmailService emailService,
        UserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<int> PlaceOrderAsync(int userId, PlaceOrderDto dto)
    {
        // Validate shipping address
        if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
            throw new Exception("Shipping address is required.");

        // Place order
        var orderId = await _orderRepository.PlaceOrderAsync(userId, dto);

        // Get full order details for email
        var orders = await _orderRepository.GetMyOrdersAsync(userId);
        var order = orders.FirstOrDefault(o => o.Id == orderId);

        // Get user details for email
        var user = await _userRepository.GetByIdAsync(userId);

        // Send confirmation email
        if (order != null && user != null)
        {
            try
            {
                await _emailService.SendOrderConfirmationAsync(
                    user.Email,
                    user.FirstName,
                    order);
            }
            catch
            {
                // Don't fail the order if email fails
            }
        }

        return orderId;
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId)
    {
        return await _orderRepository.GetMyOrdersAsync(userId);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllOrdersAsync();
    }

    public async Task<IEnumerable<UserOrderDto>> GetOrdersByUserAsync()
    {
        return await _orderRepository.GetOrdersByUserAsync();
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string status)
    {
        var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(status))
            throw new Exception("Invalid status value.");

        return await _orderRepository.UpdateStatusAsync(orderId, status);
    }
}