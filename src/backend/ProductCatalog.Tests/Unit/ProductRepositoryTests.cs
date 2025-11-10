using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Tests.Unit;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductCatalogDbContext(options);
        _repository = new ProductRepository(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var category1 = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            IsActive = true
        };

        var category2 = new Category
        {
            Id = 2,
            Name = "Books",
            Description = "Books and magazines",
            IsActive = true
        };

        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Gaming Laptop",
                Description = "High performance gaming computer",
                Price = 1499.99m,
                CategoryId = 1,
                StockQuantity = 5,
                CreatedDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Category = category1
            },
            new Product
            {
                Id = 2,
                Name = "Office Laptop",
                Description = "Business laptop for productivity",
                Price = 899.99m,
                CategoryId = 1,
                StockQuantity = 10,
                CreatedDate = new DateTime(2025, 1, 2),
                IsActive = true,
                Category = category1
            },
            new Product
            {
                Id = 3,
                Name = "Wireless Mouse",
                Description = "Ergonomic mouse for gaming",
                Price = 49.99m,
                CategoryId = 1,
                StockQuantity = 0,
                CreatedDate = new DateTime(2025, 1, 3),
                IsActive = true,
                Category = category1
            },
            new Product
            {
                Id = 4,
                Name = "Programming Book",
                Description = "Learn advanced programming concepts",
                Price = 59.99m,
                CategoryId = 2,
                StockQuantity = 20,
                CreatedDate = new DateTime(2025, 1, 4),
                IsActive = true,
                Category = category2
            },
            new Product
            {
                Id = 5,
                Name = "Inactive Product",
                Description = "This should not appear",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 100,
                CreatedDate = new DateTime(2025, 1, 5),
                IsActive = false,
                Category = category1
            }
        };

        _context.Categories.AddRange(category1, category2);
        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveProducts()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4); // 4 active products in seed data
        result.All(p => p.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Gaming Laptop");
        result.Category.Should().NotBeNull();
        result.Category.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddNewProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Description = "New product description",
            Price = 49.99m,
            CategoryId = 1,
            StockQuantity = 5,
            IsActive = true
        };

        // Act
        var result = await _repository.CreateAsync(newProduct);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Product");
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #region Search Tests

    [Fact]
    public async Task SearchAsync_WithNoParameters_ShouldReturnAllActiveProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto();

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4); // 4 active products
        result.TotalCount.Should().Be(4);
        result.Items.All(p => p.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldSearchNameCaseInsensitive()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "laptop" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Gaming Laptop and Office Laptop
        result.Items.All(p => p.Name.Contains("Laptop", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldSearchDescriptionCaseInsensitive()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "programming" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task SearchAsync_WithMultiWordSearchTerm_ShouldUseAndLogic()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "gaming laptop" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Gaming Laptop");
    }

    [Fact]
    public async Task SearchAsync_WithMultiWordSearchTerm_ShouldMatchAcrossNameAndDescription()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "gaming mouse" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Wireless Mouse");
        result.Items.First().Description.Should().Contain("gaming");
    }

    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        var searchDto = new ProductSearchDto { CategoryId = 2 };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CategoryId.Should().Be(2);
        result.Items.First().Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task SearchAsync_WithMinPriceFilter_ShouldFilterByMinPrice()
    {
        // Arrange
        var searchDto = new ProductSearchDto { MinPrice = 100m };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Gaming Laptop and Office Laptop
        result.Items.All(p => p.Price >= 100m).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithMaxPriceFilter_ShouldFilterByMaxPrice()
    {
        // Arrange
        var searchDto = new ProductSearchDto { MaxPrice = 100m };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Wireless Mouse and Programming Book
        result.Items.All(p => p.Price <= 100m).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithPriceRange_ShouldFilterByBothMinAndMaxPrice()
    {
        // Arrange
        var searchDto = new ProductSearchDto { MinPrice = 50m, MaxPrice = 1000m };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Office Laptop and Programming Book
        result.Items.All(p => p.Price >= 50m && p.Price <= 1000m).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithInStockTrue_ShouldFilterInStockProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto { InStock = true };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3); // All except Wireless Mouse (0 stock)
        result.Items.All(p => p.StockQuantity > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_WithInStockFalse_ShouldReturnAllProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto { InStock = false };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4); // All active products regardless of stock
    }

    [Fact]
    public async Task SearchAsync_WithSortByNameAsc_ShouldSortCorrectly()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SortBy = "Name", SortOrder = "asc" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        var names = result.Items.Select(p => p.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_WithSortByPriceDesc_ShouldSortCorrectly()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SortBy = "Price", SortOrder = "desc" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        var prices = result.Items.Select(p => p.Price).ToList();
        prices.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchAsync_WithSortByCreatedAsc_ShouldSortCorrectly()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SortBy = "Created", SortOrder = "asc" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        var dates = result.Items.Select(p => p.CreatedDate).ToList();
        dates.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var searchDto = new ProductSearchDto { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_WithSecondPage_ShouldReturnCorrectItems()
    {
        // Arrange
        var searchDto = new ProductSearchDto { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task SearchAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var searchDto = new ProductSearchDto
        {
            SearchTerm = "laptop",
            CategoryId = 1,
            MinPrice = 800m,
            MaxPrice = 2000m,
            InStock = true,
            SortBy = "Price",
            SortOrder = "desc",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.All(p => p.CategoryId == 1 && p.Price >= 800m && p.Price <= 2000m && p.StockQuantity > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ShouldIncludeCategoryNavigationProperty()
    {
        // Arrange
        var searchDto = new ProductSearchDto();

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        result.Items.All(p => p.Category != null).Should().BeTrue();
        result.Items.First().Category!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldOnlyReturnActiveProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto();

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.All(p => p.IsActive).Should().BeTrue();
        result.Items.Should().NotContain(p => p.Id == 5); // Inactive product
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "nonexistent product xyz123" };

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}