using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Tests.Fixtures;

public static class TestDbContextFactory
{
    public static ProductCatalogDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ProductCatalogDbContext(options);
    }

    public static async Task<ProductCatalogDbContext> CreateSeededDbContextAsync(string? databaseName = null)
    {
        var context = CreateInMemoryDbContext(databaseName);
        await SeedTestDataAsync(context);
        return context;
    }

    private static async Task SeedTestDataAsync(ProductCatalogDbContext context)
    {
        // Add test categories
        var electronics = new ProductCatalog.Core.Entities.Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and gadgets",
            IsActive = true
        };

        var books = new ProductCatalog.Core.Entities.Category
        {
            Id = 2,
            Name = "Books",
            Description = "Books and literature",
            IsActive = true
        };

        context.Categories.AddRange(electronics, books);

        // Add test products
        var products = new[]
        {
            new ProductCatalog.Core.Entities.Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "Gaming laptop",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 10,
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new ProductCatalog.Core.Entities.Product
            {
                Id = 2,
                Name = "Programming Book",
                Description = "Learn C# programming",
                Price = 49.99m,
                CategoryId = 2,
                StockQuantity = 5,
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            },
            new ProductCatalog.Core.Entities.Product
            {
                Id = 3,
                Name = "Inactive Product",
                Description = "This product is inactive",
                Price = 19.99m,
                CategoryId = 1,
                StockQuantity = 0,
                CreatedDate = DateTime.UtcNow.AddDays(-60),
                IsActive = false
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}