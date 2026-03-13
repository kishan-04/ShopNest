using Dapper;
using ShopNest.API.Data;
using ShopNest.API.DTOs;

namespace ShopNest.API.Repositories;

public class DashboardRepository
{
    private readonly DapperContext _context;

    public DashboardRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardDataAsync()
    {
        using var connection = _context.CreateConnection();

        // Total counts
        var totalUsers = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Users WHERE Role = 'Customer'");

        var totalOrders = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Orders");

        var totalSales = await connection.ExecuteScalarAsync<decimal>(
            "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders");

        var totalProducts = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Products WHERE IsActive = 1");

        // Orders by status
        var ordersByStatus = (await connection.QueryAsync<OrderStatusCountDto>(@"
            SELECT Status, COUNT(*) AS Count
            FROM Orders
            GROUP BY Status")).ToList();

        // Sales last 7 days
        var salesLast7Days = (await connection.QueryAsync<DailySalesDto>(@"
            SELECT
                FORMAT(CreatedAt, 'dd MMM') AS Day,
                ISNULL(SUM(TotalAmount), 0)  AS Total
            FROM Orders
            WHERE CreatedAt >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
            GROUP BY FORMAT(CreatedAt, 'dd MMM'), CAST(CreatedAt AS DATE)
            ORDER BY CAST(CreatedAt AS DATE)")).ToList();

        // Recent 5 orders
        var recentOrders = (await connection.QueryAsync<RecentOrderDto>(@"
            SELECT TOP 5
                o.Id,
                u.FirstName + ' ' + u.LastName AS CustomerName,
                o.TotalAmount,
                o.Status,
                o.CreatedAt
            FROM Orders o
            INNER JOIN Users u ON u.Id = o.UserId
            ORDER BY o.CreatedAt DESC")).ToList();

        return new DashboardDto
        {
            TotalUsers = totalUsers,
            TotalOrders = totalOrders,
            TotalSales = totalSales,
            TotalProducts = totalProducts,
            OrdersByStatus = ordersByStatus,
            SalesLast7Days = salesLast7Days,
            RecentOrders = recentOrders
        };
    }
}