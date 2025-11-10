using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;

namespace ProductCatalog.Tests.Fixtures;

public static class TestDataBuilder
{
    public static Product CreateTestProduct(
        int id = 1,
        string name = "Test Product",
        string description = "Test Description",
        decimal price = 99.99m,
        int categoryId = 1,
        int stockQuantity = 10,
        bool isActive = true)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity,
            CreatedDate = DateTime.UtcNow,
            IsActive = isActive
        };
    }

    public static Category CreateTestCategory(
        int id = 1,
        string name = "Test Category",
        string description = "Test Description",
        bool isActive = true)
    {
        return new Category
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = isActive
        };
    }

    public static CreateProductDto CreateProductDto(
        string name = "Test Product",
        string description = "Test Description",
        decimal price = 99.99m,
        int categoryId = 1,
        int stockQuantity = 10)
    {
        return new CreateProductDto
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity
        };
    }

    public static UpdateProductDto UpdateProductDto(
        string name = "Updated Product",
        string description = "Updated Description",
        decimal price = 149.99m,
        int categoryId = 1,
        int stockQuantity = 15,
        bool isActive = true)
    {
        return new UpdateProductDto
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity,
            IsActive = isActive
        };
    }

    public static CreateCategoryDto CreateCategoryDto(
        string name = "Test Category",
        string description = "Test Description")
    {
        return new CreateCategoryDto
        {
            Name = name,
            Description = description
        };
    }

    public static ProductSearchDto CreateProductSearchDto(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        string sortBy = "Name",
        string sortOrder = "asc",
        int pageNumber = 1,
        int pageSize = 10)
    {
        return new ProductSearchDto
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStock = inStock,
            SortBy = sortBy,
            SortOrder = sortOrder,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}