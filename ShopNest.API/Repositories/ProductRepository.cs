using Dapper;
using ShopNest.API.Data;
using ShopNest.API.DTOs;
using ShopNest.API.Models;

namespace ShopNest.API.Repositories;

public class ProductRepository
{
    private readonly DapperContext _context;

    public ProductRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var sql = "SELECT Id, Description, ImageUrl, Stock, Name, Price, CategoryId FROM Products";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var sql = @"
        SELECT
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.IsActive,
            p.CategoryId,
            p.CreatedAt,
            c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.Id = @Id AND p.IsActive = 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(ProductFilterDto filter)
    {
        var sql = @"
        SELECT
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.IsActive,
            p.CategoryId,
            p.CreatedAt,
            c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.IsActive = 1
          AND (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
        ORDER BY p.CreatedAt DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*)
        FROM Products p
        WHERE p.IsActive = 1
          AND (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId);";

        var parameters = new
        {
            Search = filter.Search,
            CategoryId = filter.CategoryId,
            Skip = (filter.Page - 1) * filter.PageSize,
            PageSize = filter.PageSize
        };

        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, parameters);

        var products = await multi.ReadAsync<Product>();
        var totalCount = await multi.ReadFirstAsync<int>();

        return (products, totalCount);
    }

    public async Task<int> CreateAsync(CreateProductDto dto)
    {
        var sql = @"
        INSERT INTO Products (Name, Description, ImageUrl, Price, Stock, CategoryId)
        OUTPUT INSERTED.Id
        VALUES (@Name, @Description, @ImageUrl, @Price, @Stock, @CategoryId)";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, dto);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var sql = @"
        UPDATE Products
        SET Name       = @Name,
            Description= @Description,
            ImageUrl   = @ImageUrl,
            Stock      = @Stock,
            Price      = @Price,
            CategoryId = @CategoryId
        WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            dto.Name,
            dto.Description,
            dto.ImageUrl,
            dto.Stock,
            dto.Price,
            dto.CategoryId,
            Id = id
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sql = "UPDATE Products SET IsActive = 0 WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}