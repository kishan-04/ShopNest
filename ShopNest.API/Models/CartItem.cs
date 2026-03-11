namespace ShopNest.API.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime AddedAt { get; set; }

    // Populated by JOIN — not real columns
    public string? ProductName { get; set; }
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
}