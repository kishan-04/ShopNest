using Dapper;
using ShopNest.API.Data;
using ShopNest.API.Models;

namespace ShopNest.API.Repositories;

public class UserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var sql = "SELECT * FROM Users WHERE Email = @Email";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task<int> CreateAsync(User user)
    {
        var sql = @"
            INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Email, @PasswordHash, @Role)";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var sql = "SELECT * FROM Users WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Step 1 — Delete cart (CartItems auto deleted by CASCADE)
            await connection.ExecuteAsync(
                "DELETE FROM Carts WHERE UserId = @Id",
                new { Id = id }, transaction);

            // Step 2 — Delete orders (OrderItems auto deleted by CASCADE)
            await connection.ExecuteAsync(
                "DELETE FROM Orders WHERE UserId = @Id",
                new { Id = id }, transaction);

            // Step 3 — Delete user
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Users WHERE Id = @Id AND Role = 'Customer'",
                new { Id = id }, transaction);

            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}