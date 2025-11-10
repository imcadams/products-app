using FluentAssertions;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;
using ProductCatalog.Tests.Fixtures;

namespace ProductCatalog.Tests.Integration;

public class ProductRepositoryIntegrationTests : IDisposable
{
    private readonly ProductCatalogDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _repository.GetAllActiveAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only active products
        result.Should().OnlyContain(p => p.IsActive);
        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Programming Book");
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        await SeedTestDataAsync();
        var searchDto = TestDataBuilder.CreateProductSearchDto(searchTerm: "Laptop");

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ShouldReturnProductsInCategory()
    {
        // Arrange
        await SeedTestDataAsync();
        var searchDto = TestDataBuilder.CreateProductSearchDto(categoryId: 1);

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Only active products in category 1
        result.Items.First().CategoryId.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_WithPriceRange_ShouldReturnProductsInRange()
    {
        // Arrange
        await SeedTestDataAsync();
        var searchDto = TestDataBuilder.CreateProductSearchDto(minPrice: 40m, maxPrice: 100m);

        // Act
        var result = await _repository.SearchAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Only the programming book (49.99)
        result.Items.Should().OnlyContain(p => p.Price >= 40m && p.Price <= 100m);
        result.Items.First().Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        await SeedTestDataAsync();
        var product = TestDataBuilder.CreateTestProduct(id: 0, name: "New Product", categoryId: 1);

        // Act
        var result = await _repository.CreateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Product");

        // Verify in database - use Find which doesn't filter by IsActive
        var fromDb = await _context.Products.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("New Product");
        fromDb.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingProduct()
    {
        // Arrange
        await SeedTestDataAsync();
        var product = await _repository.GetByIdAsync(1);
        product!.Name = "Updated Laptop";
        product.Price = 1299.99m;

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Laptop");
        result.Price.Should().Be(1299.99m);

        // Verify in database
        var fromDb = await _repository.GetByIdAsync(1);
        fromDb!.Name.Should().Be("Updated Laptop");
        fromDb.Price.Should().Be(1299.99m);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkProductAsInactive()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _repository.SoftDeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify product is marked as inactive
        var product = await _context.Products.FindAsync(1);
        product.Should().NotBeNull();
        product!.IsActive.Should().BeFalse();

        // Verify it doesn't appear in active products
        var activeProducts = await _repository.GetAllActiveAsync();
        activeProducts.Should().NotContain(p => p.Id == 1);
    }

    private async Task SeedTestDataAsync()
    {
        await TestDbContextFactory.CreateSeededDbContextAsync();
        
        // Copy seeded data to our context
        var electronics = TestDataBuilder.CreateTestCategory(1, "Electronics", "Electronic devices and gadgets");
        var books = TestDataBuilder.CreateTestCategory(2, "Books", "Books and literature");
        
        _context.Categories.AddRange(electronics, books);

        var laptop = TestDataBuilder.CreateTestProduct(1, "Laptop", "Gaming laptop", 999.99m, 1, 10);
        var book = TestDataBuilder.CreateTestProduct(2, "Programming Book", "Learn C# programming", 49.99m, 2, 5);
        var inactive = TestDataBuilder.CreateTestProduct(3, "Inactive Product", "This product is inactive", 19.99m, 1, 0, false);

        _context.Products.AddRange(laptop, book, inactive);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}