namespace ShopNest.API.DTOs;

// What Angular sends when adding item to cart
public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;

}