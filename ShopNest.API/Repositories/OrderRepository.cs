using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using ShopNest.API.Data;
using ShopNest.API.DTOs;
using ShopNest.API.Models;
using System.Collections.Generic;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShopNest.API.Repositories;

public class OrderRepository
{
    private readonly DapperContext _context;

    public OrderRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<int> PlaceOrderAsync(int userId, PlaceOrderDto dto)
    {
        // Open connection manually — needed for transaction
        using var connection = _context.CreateConnection();
        connection.Open();

        // Begin transaction
        using var transaction = connection.BeginTransaction();

        try
        {
            // ── Step 1: Get user's cart ────────────────────────
            var cartSql = @"
                SELECT c.Id AS CartId, ci.ProductId, ci.Quantity, ci.UnitPrice
                FROM Carts c
                INNER JOIN CartItems ci ON ci.CartId = c.Id
                WHERE c.UserId = @UserId";

            var cartItems = (await connection.QueryAsync<dynamic>(
                cartSql,
                new { UserId = userId },
                transaction)).ToList();

            if (!cartItems.Any())
                throw new Exception("Your cart is empty.");

            // ── Step 2: Calculate total ────────────────────────
            decimal totalAmount = 0;
            foreach (var item in cartItems)
                totalAmount += (decimal)item.UnitPrice * (int)item.Quantity;

            // ── Step 3: Create Order ───────────────────────────
            var createOrderSql = @"
                INSERT INTO Orders (UserId, TotalAmount, Status, ShippingAddress)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @TotalAmount, 'Pending', @ShippingAddress)";

            var orderId = await connection.ExecuteScalarAsync<int>(
                createOrderSql,
                new
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    dto.ShippingAddress
                },
                transaction);

            // ── Step 4: Insert OrderItems & Reduce Stock ───────
            foreach (var item in cartItems)
            {
                // Insert order item
                await connection.ExecuteAsync(@"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)",
                    new
                    {
                        OrderId = orderId,
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice
                    },
                    transaction);

                // Reduce stock
                var rowsAffected = await connection.ExecuteAsync(@"
                    UPDATE Products
                    SET Stock = Stock - @Quantity
                    WHERE Id = @ProductId AND Stock >= @Quantity",
                    new { item.ProductId, item.Quantity },
                    transaction);

                // If rowsAffected = 0 means not enough stock
                if (rowsAffected == 0)
                    throw new Exception($"Not enough stock for product ID {item.ProductId}.");
            }

            // ── Step 5: Clear Cart ─────────────────────────────
            var cartId = (int)cartItems[0].CartId;
            await connection.ExecuteAsync(
                "DELETE FROM CartItems WHERE CartId = @CartId",
                new { CartId = cartId },
                transaction);

            // ── All steps succeeded → COMMIT ───────────────────
            transaction.Commit();
            return orderId;
        }
        catch
        {
            // Something failed → ROLLBACK everything
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(int userId)
    {
        var sql = @"
        SELECT
            o.Id,
            o.TotalAmount,
            o.Status,
            o.ShippingAddress,
            o.CreatedAt,
            oi.ProductId,
            p.Name      AS ProductName,
            c.Name      AS CategoryName,
            oi.Quantity,
            oi.UnitPrice
        FROM Orders o
        INNER JOIN OrderItems oi ON oi.OrderId   = o.Id
        INNER JOIN Products   p  ON p.Id         = oi.ProductId
        INNER JOIN Categories c  ON c.Id         = p.CategoryId
        WHERE o.UserId = @UserId
        ORDER BY o.CreatedAt DESC";

        using var connection = _context.CreateConnection();

        // Use dynamic first then map manually
        var rows = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });

        var orderDict = new Dictionary<int, OrderDto>();

        foreach (var row in rows)
        {
            int orderId = (int)row.Id;

            if (!orderDict.TryGetValue(orderId, out var order))
            {
                order = new OrderDto
                {
                    Id = row.Id,
                    TotalAmount = row.TotalAmount,
                    Status = row.Status,
                    ShippingAddress = row.ShippingAddress,
                    CreatedAt = row.CreatedAt,
                    Items = new List<OrderItemDto>()
                };
                orderDict.Add(orderId, order);
            }

            order.Items.Add(new OrderItemDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                CategoryName = row.CategoryName,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice
            });
        }

        return orderDict.Values;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var sql = @"
        SELECT
            o.Id,
            o.TotalAmount,
            o.Status,
            o.ShippingAddress,
            o.CreatedAt,
            oi.ProductId,
            p.Name      AS ProductName,
            c.Name      AS CategoryName,
            oi.Quantity,
            oi.UnitPrice
        FROM Orders o
        INNER JOIN OrderItems oi ON oi.OrderId = o.Id
        INNER JOIN Products   p  ON p.Id       = oi.ProductId
        INNER JOIN Categories c  ON c.Id       = p.CategoryId
        ORDER BY o.CreatedAt DESC";

        using var connection = _context.CreateConnection();

        var rows = await connection.QueryAsync<dynamic>(sql);

        var orderDict = new Dictionary<int, OrderDto>();

        foreach (var row in rows)
        {
            int orderId = (int)row.Id;

            if (!orderDict.TryGetValue(orderId, out var order))
            {
                order = new OrderDto
                {
                    Id = row.Id,
                    TotalAmount = row.TotalAmount,
                    Status = row.Status,
                    ShippingAddress = row.ShippingAddress,
                    CreatedAt = row.CreatedAt,
                    Items = new List<OrderItemDto>()
                };
                orderDict.Add(orderId, order);
            }

            order.Items.Add(new OrderItemDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                CategoryName = row.CategoryName,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice
            });
        }

        return orderDict.Values;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string status)
    {
        var sql = @"
            UPDATE Orders
            SET Status = @Status
            WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(
            sql, new { Id = orderId, Status = status });

        return rowsAffected > 0;
    }

    public async Task<IEnumerable<UserOrderDto>> GetOrdersByUserAsync()
    {
        var sql = @"
        SELECT
            u.Id        AS UserId,
            u.FirstName,
            u.LastName,
            u.Email,
            o.Id,
            o.TotalAmount,
            o.Status,
            o.ShippingAddress,
            o.CreatedAt,
            oi.ProductId,
            p.Name      AS ProductName,
            c.Name      AS CategoryName,
            oi.Quantity,
            oi.UnitPrice
        FROM Users u
        LEFT JOIN Orders o      ON o.UserId    = u.Id
        LEFT JOIN OrderItems oi ON oi.OrderId  = o.Id
        LEFT JOIN Products p    ON p.Id        = oi.ProductId
        LEFT JOIN Categories c  ON c.Id        = p.CategoryId
        WHERE u.Role = 'Customer'
        ORDER BY u.Id, o.CreatedAt DESC";

        using var connection = _context.CreateConnection();
        var rows = await connection.QueryAsync<dynamic>(sql);

        var userDict = new Dictionary<int, UserOrderDto>();

        foreach (var row in rows)
        {
            int userId = (int)row.UserId;

            if (!userDict.TryGetValue(userId, out var userOrder))
            {
                userOrder = new UserOrderDto
                {
                    UserId = row.UserId,
                    FirstName = row.FirstName,
                    LastName = row.LastName,
                    Email = row.Email,
                    Orders = new List<OrderDto>()
                };
                userDict.Add(userId, userOrder);
            }

            // Skip if user has no orders
            if (row.Id == null) continue;

            int orderId = (int)row.Id;
            var existingOrder = userOrder.Orders.FirstOrDefault(o => o.Id == orderId);

            if (existingOrder == null)
            {
                existingOrder = new OrderDto
                {
                    Id = row.Id,
                    TotalAmount = row.TotalAmount,
                    Status = row.Status,
                    ShippingAddress = row.ShippingAddress,
                    CreatedAt = row.CreatedAt,
                    Items = new List<OrderItemDto>()
                };
                userOrder.Orders.Add(existingOrder);
            }

            // Skip if no items
            if (row.ProductId == null) continue;

            existingOrder.Items.Add(new OrderItemDto
            {
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                CategoryName = row.CategoryName,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice
            });
        }

        // Calculate totals
        foreach (var user in userDict.Values)
        {
            user.TotalOrders = user.Orders.Count;
            user.TotalSpent = user.Orders.Sum(o => o.TotalAmount);
        }

        return userDict.Values;
    }
}