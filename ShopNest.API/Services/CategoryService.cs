using ShopNest.API.DTOs;
using ShopNest.API.Models;
using ShopNest.API.Repositories;

namespace ShopNest.API.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService(CategoryRepository categoryRepository) 
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }
    }
}
