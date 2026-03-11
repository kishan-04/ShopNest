using ShopNest.API.DTOs;
using ShopNest.API.Models;
using ShopNest.API.Repositories;

namespace ShopNest.API.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<PagedResultDto<Product>> GetPagedAsync(ProductFilterDto filter)
    {
        // Validate page number
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 6;
        if (filter.PageSize > 50) filter.PageSize = 50; // max 50 per page

        var (products, totalCount) = await _productRepository.GetPagedAsync(filter);

        return new PagedResultDto<Product>
        {
            Data = products,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<int> CreateAsync(CreateProductDto dto)
    {
        // Business rules — check before saving
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Product name cannot be empty.");

        if (dto.Price <= 0)
            throw new Exception("Price must be greater than zero.");

        if (dto.Stock < 0)
            throw new Exception("Stock must be greater than zero.");

        if (dto.CategoryId <= 0)
            throw new Exception("Please select a valid category.");

        return await _productRepository.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        // Check product exists first
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null)
            return false;

        // Business rules
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Product name cannot be empty.");

        if (dto.Price <= 0)
            throw new Exception("Price must be greater than zero.");

        if (dto.Stock < 0)
            throw new Exception("Stock must be greater than zero.");

        if (dto.CategoryId <= 0)
            throw new Exception("Please select a valid category.");

        return await _productRepository.UpdateAsync(id, dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Check product exists first
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _productRepository.DeleteAsync(id);
    }
}