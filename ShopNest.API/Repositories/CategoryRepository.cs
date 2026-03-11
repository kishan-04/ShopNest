using Dapper;
using ShopNest.API.Data;
using ShopNest.API.Models;

namespace ShopNest.API.Repositories;

public class CategoryRepository
{
    private readonly DapperContext _context;

    public CategoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var sql = "SELECT Id, Name, CreatedAt FROM Categories ORDER BY Name";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Category>(sql);
    }
}