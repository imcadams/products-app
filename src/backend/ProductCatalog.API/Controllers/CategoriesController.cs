using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all active categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
        var categories = await _categoryService.GetAllActiveCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CreateCategoryDto updateCategoryDto)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
        return Ok(category);
    }

    /// <summary>
    /// Soft delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.SoftDeleteCategoryAsync(id);
        if (!result)
        {
            return NotFound($"Category with ID {id} was not found or cannot be deleted");
        }

        return NoContent();
    }
}