namespace ShopNest.API.DTOs;

public class DashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalProducts { get; set; }
    public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
    public List<DailySalesDto> SalesLast7Days { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class OrderStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailySalesDto
{
    public string Day { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class RecentOrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}