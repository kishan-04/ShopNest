namespace ShopNest.API.DTOs;

// What Angular sends when placing an order
public class PlaceOrderDto
{
    public string ShippingAddress { get; set; } = string.Empty;
}