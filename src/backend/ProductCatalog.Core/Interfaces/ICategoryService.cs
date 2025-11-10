using ProductCatalog.Core.DTOs;

namespace ProductCatalog.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllActiveCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto);
    Task<bool> SoftDeleteCategoryAsync(int id);
}