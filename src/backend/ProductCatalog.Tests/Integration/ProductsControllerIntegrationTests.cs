using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Tests.Fixtures;
using System.Net.Http.Json;
using System.Text.Json;
using ProductCatalog.Core.DTOs;

namespace ProductCatalog.Tests.Integration;

public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _databaseName;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Use a unique database name for each test class instance to avoid test pollution
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ProductCatalogDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing with unique name
                services.AddDbContext<ProductCatalogDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Build service provider and seed data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
                SeedTestData(context);
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnActiveProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<ProductDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        products.Should().NotBeNull();
        products.Should().HaveCount(2); // Only active products
        products.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        product.Should().NotBeNull();
        product!.Id.Should().Be(1);
        product.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateProductDto(
            name: "New Test Product",
            description: "Created via integration test",
            price: 199.99m,
            categoryId: 1,
            stockQuantity: 20);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        product.Should().NotBeNull();
        product!.Name.Should().Be("New Test Product");
        product.Price.Should().Be(199.99m);
        product.CategoryId.Should().Be(1);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "", // Invalid: empty name
            Description = "Test",
            Price = -10, // Invalid: negative price
            CategoryId = 999, // Invalid: non-existent category
            StockQuantity = -5 // Invalid: negative stock
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createDto);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProducts_WithNoParameters_ShouldReturnAllActiveProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2); // Only active products
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task SearchProducts_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=laptop");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchProducts_WithSearchTermCaseInsensitive_ShouldReturnMatchingProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=LAPTOP");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchProducts_WithMultiWordSearchTerm_ShouldUseAndLogic()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=gaming%20laptop");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchProducts_WithSearchTermInDescription_ShouldReturnMatchingProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=programming");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task SearchProducts_WithCategoryId_ShouldFilterByCategory()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?categoryId=2");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().CategoryId.Should().Be(2);
    }

    [Fact]
    public async Task SearchProducts_WithMinPrice_ShouldFilterByMinPrice()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?minPrice=100");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.All(p => p.Price >= 100m).Should().BeTrue();
    }

    [Fact]
    public async Task SearchProducts_WithMaxPrice_ShouldFilterByMaxPrice()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?maxPrice=100");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.All(p => p.Price <= 100m).Should().BeTrue();
    }

    [Fact]
    public async Task SearchProducts_WithPriceRange_ShouldFilterByBothPrices()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?minPrice=40&maxPrice=60");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task SearchProducts_WithInStockTrue_ShouldFilterInStockProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?inStock=true");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items.All(p => p.StockQuantity > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchProducts_WithSortByName_ShouldSortByName()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?sortBy=Name&sortOrder=asc");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        var names = result.Items.Select(p => p.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchProducts_WithSortByPriceDesc_ShouldSortByPriceDescending()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?sortBy=Price&sortOrder=desc");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        var prices = result.Items.Select(p => p.Price).ToList();
        prices.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchProducts_WithPagination_ShouldReturnCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?pageNumber=1&pageSize=1");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task SearchProducts_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?searchTerm=laptop&categoryId=1&minPrice=500&maxPrice=1500&inStock=true&sortBy=Price&sortOrder=asc&pageNumber=1&pageSize=10");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task SearchProducts_ResponseStructure_ShouldMatchSpecification()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search");

        // Assert
        response.Should().BeSuccessful();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.PageNumber.Should().BeGreaterThan(0);
        result.PageSize.Should().BeGreaterThan(0);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    private static void SeedTestData(ProductCatalogDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Categories.Any()) return; // Already seeded

        var electronics = TestDataBuilder.CreateTestCategory(1, "Electronics", "Electronic devices and gadgets");
        var books = TestDataBuilder.CreateTestCategory(2, "Books", "Books and literature");

        context.Categories.AddRange(electronics, books);

        var laptop = TestDataBuilder.CreateTestProduct(1, "Laptop", "Gaming laptop", 999.99m, 1, 10);
        var book = TestDataBuilder.CreateTestProduct(2, "Programming Book", "Learn C# programming", 49.99m, 2, 5);
        var inactive = TestDataBuilder.CreateTestProduct(3, "Inactive Product", "This product is inactive", 19.99m, 1, 0, false);

        context.Products.AddRange(laptop, book, inactive);
        context.SaveChanges();
    }
}