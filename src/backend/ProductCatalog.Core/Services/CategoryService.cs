using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Core.Exceptions;

namespace ProductCatalog.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllActiveAsync();
        return categories.Select(MapToCategoryDto);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException($"Category with ID {id} was not found");
            
        return MapToCategoryDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            IsActive = true
        };

        var createdCategory = await _categoryRepository.CreateAsync(category);
        return MapToCategoryDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
            throw new NotFoundException($"Category with ID {id} was not found");

        existingCategory.Name = updateCategoryDto.Name;
        existingCategory.Description = updateCategoryDto.Description;

        var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
        return MapToCategoryDto(updatedCategory);
    }

    public async Task<bool> SoftDeleteCategoryAsync(int id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }

    private static CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        };
    }
}