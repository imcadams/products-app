using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Interfaces;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var products = await _productService.GetAllActiveProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var product = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var product = await _productService.UpdateProductAsync(id, updateProductDto);
        return Ok(product);
    }

    /// <summary>
    /// Soft delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.SoftDeleteProductAsync(id);
        if (!result)
        {
            return NotFound($"Product with ID {id} was not found");
        }

        return NoContent();
    }

    /// <summary>
    /// Search products with filters and pagination
    /// </summary>
    /// <param name="searchDto">Search parameters including:
    /// - searchTerm: Case-insensitive search across name and description (multi-word AND logic)
    /// - categoryId: Filter by category
    /// - minPrice: Minimum price filter
    /// - maxPrice: Maximum price filter
    /// - inStock: Filter by stock availability (true = only in stock)
    /// - sortBy: Sort field (Name, Price, Created)
    /// - sortOrder: Sort direction (asc, desc)
    /// - pageNumber: Page number (default: 1)
    /// - pageSize: Items per page (default: 10)
    /// </param>
    /// <returns>Paged result with items, totalCount, pageNumber, pageSize, and totalPages</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> SearchProducts([FromQuery] ProductSearchDto searchDto)
    {
        var result = await _productService.SearchProductsAsync(searchDto);
        return Ok(result);
    }
}