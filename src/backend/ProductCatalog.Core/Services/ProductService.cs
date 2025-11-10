using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Core.Exceptions;

namespace ProductCatalog.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllActiveProductsAsync()
    {
        var products = await _productRepository.GetAllActiveAsync();
        return products.Select(MapToProductDto);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException($"Product with ID {id} was not found");
            
        return MapToProductDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        // Validate category exists
        if (!await _categoryRepository.ExistsAsync(createProductDto.CategoryId))
        {
            throw new NotFoundException($"Category with ID {createProductDto.CategoryId} not found");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            CategoryId = createProductDto.CategoryId,
            StockQuantity = createProductDto.StockQuantity,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var createdProduct = await _productRepository.CreateAsync(product);
        return MapToProductDto(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }

        // Validate category exists
        if (!await _categoryRepository.ExistsAsync(updateProductDto.CategoryId))
        {
            throw new NotFoundException($"Category with ID {updateProductDto.CategoryId} not found");
        }

        existingProduct.Name = updateProductDto.Name;
        existingProduct.Description = updateProductDto.Description;
        existingProduct.Price = updateProductDto.Price;
        existingProduct.CategoryId = updateProductDto.CategoryId;
        existingProduct.StockQuantity = updateProductDto.StockQuantity;
        existingProduct.IsActive = updateProductDto.IsActive;

        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return MapToProductDto(updatedProduct);
    }

    public async Task<bool> SoftDeleteProductAsync(int id)
    {
        var exists = await _productRepository.ExistsAsync(id);
        if (!exists)
            throw new NotFoundException($"Product with ID {id} was not found");

        return await _productRepository.SoftDeleteAsync(id);
    }

    public async Task<PagedResultDto<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
    {
        var result = await _productRepository.SearchAsync(searchDto);
        return new PagedResultDto<ProductDto>
        {
            Items = result.Items.Select(MapToProductDto),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            StockQuantity = product.StockQuantity,
            CreatedDate = product.CreatedDate,
            IsActive = product.IsActive
        };
    }
}