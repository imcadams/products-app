using ProductCatalog.Core.Entities;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ProductCatalogDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (context.Categories.Any())
        {
            return; // Database has been seeded
        }

        // Create categories
        var categories = new List<Category>
        {
            new Category { Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true },
            new Category { Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
            new Category { Name = "Books", Description = "Books and educational materials", IsActive = true },
            new Category { Name = "Home & Garden", Description = "Home improvement and gardening items", IsActive = true },
            new Category { Name = "Sports", Description = "Sports and fitness equipment", IsActive = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Create products
        var products = new List<Product>
        {
            // Electronics (5 products)
            new Product { Name = "Laptop", Description = "High-performance laptop computer", Price = 999.99m, CategoryId = categories[0].Id, StockQuantity = 50, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Smartphone", Description = "Latest model smartphone", Price = 699.99m, CategoryId = categories[0].Id, StockQuantity = 100, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Wireless Earbuds", Description = "Premium wireless earbuds", Price = 149.99m, CategoryId = categories[0].Id, StockQuantity = 200, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Gaming Monitor", Description = "27-inch 4K gaming monitor", Price = 449.99m, CategoryId = categories[0].Id, StockQuantity = 30, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 29.99m, CategoryId = categories[0].Id, StockQuantity = 150, CreatedDate = DateTime.UtcNow, IsActive = true },
            
            // Clothing (4 products)
            new Product { Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, CategoryId = categories[1].Id, StockQuantity = 500, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Jeans", Description = "Denim jeans", Price = 59.99m, CategoryId = categories[1].Id, StockQuantity = 150, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Sneakers", Description = "Running sneakers", Price = 89.99m, CategoryId = categories[1].Id, StockQuantity = 75, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Winter Jacket", Description = "Insulated winter jacket", Price = 129.99m, CategoryId = categories[1].Id, StockQuantity = 35, CreatedDate = DateTime.UtcNow, IsActive = true },
            
            // Books (4 products)
            new Product { Name = "Programming Book", Description = "Learn programming fundamentals", Price = 39.99m, CategoryId = categories[2].Id, StockQuantity = 80, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Science Fiction Novel", Description = "Bestselling sci-fi novel", Price = 14.99m, CategoryId = categories[2].Id, StockQuantity = 120, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Cookbook", Description = "Healthy cooking recipes", Price = 24.99m, CategoryId = categories[2].Id, StockQuantity = 60, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Mystery Novel", Description = "Gripping mystery thriller", Price = 18.99m, CategoryId = categories[2].Id, StockQuantity = 95, CreatedDate = DateTime.UtcNow, IsActive = true },
            
            // Home & Garden (4 products)
            new Product { Name = "Coffee Maker", Description = "Automatic drip coffee maker", Price = 79.99m, CategoryId = categories[3].Id, StockQuantity = 40, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Garden Tools Set", Description = "Complete gardening tool kit", Price = 49.99m, CategoryId = categories[3].Id, StockQuantity = 25, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Throw Pillow", Description = "Decorative throw pillow", Price = 12.99m, CategoryId = categories[3].Id, StockQuantity = 200, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "LED Desk Lamp", Description = "Adjustable LED desk lamp", Price = 34.99m, CategoryId = categories[3].Id, StockQuantity = 65, CreatedDate = DateTime.UtcNow, IsActive = true },
            
            // Sports (3 products)
            new Product { Name = "Yoga Mat", Description = "Non-slip yoga mat", Price = 29.99m, CategoryId = categories[4].Id, StockQuantity = 100, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Dumbbells", Description = "Adjustable dumbbells set", Price = 199.99m, CategoryId = categories[4].Id, StockQuantity = 20, CreatedDate = DateTime.UtcNow, IsActive = true },
            new Product { Name = "Basketball", Description = "Official size basketball", Price = 34.99m, CategoryId = categories[4].Id, StockQuantity = 50, CreatedDate = DateTime.UtcNow, IsActive = true }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}