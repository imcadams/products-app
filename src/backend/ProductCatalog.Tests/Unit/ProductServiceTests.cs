using FluentAssertions;
using Moq;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Exceptions;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Core.Services;

namespace ProductCatalog.Tests.Unit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _service = new ProductService(_mockProductRepository.Object, _mockCategoryRepository.Object);
    }

    [Fact]
    public async Task GetAllActiveProductsAsync_ShouldReturnMappedProductDtos()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                CategoryId = 1,
                StockQuantity = 10,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                Category = new Category { Id = 1, Name = "Test Category", IsActive = true }
            }
        };

        _mockProductRepository
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _service.GetAllActiveProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var productDto = result.First();
        productDto.Id.Should().Be(1);
        productDto.Name.Should().Be("Test Product");
        productDto.Price.Should().Be(99.99m);
        productDto.CategoryName.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CategoryId = 1,
            StockQuantity = 10,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Category = new Category { Id = 1, Name = "Test Category", IsActive = true }
        };

        _mockProductRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.CategoryName.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockProductRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetProductByIdAsync(999))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with ID 999 was not found");
    }

    [Fact]
    public async Task CreateProductAsync_WithValidDto_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 49.99m,
            CategoryId = 1,
            StockQuantity = 5
        };

        var createdProduct = new Product
        {
            Id = 2,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId,
            StockQuantity = createDto.StockQuantity,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            Category = new Category { Id = 1, Name = "Test Category", IsActive = true }
        };

        _mockCategoryRepository
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        _mockProductRepository
            .Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(49.99m);
        result.CategoryName.Should().Be("Test Category");

        _mockProductRepository.Verify(r => r.CreateAsync(It.Is<Product>(p => 
            p.Name == createDto.Name && 
            p.Price == createDto.Price)), Times.Once);
    }

    #region Search Tests

    [Fact]
    public async Task SearchProductsAsync_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SearchTerm = "laptop" };
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop Computer",
                Description = "High performance",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 5,
                IsActive = true,
                Category = new Category { Id = 1, Name = "Electronics" }
            }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        _mockProductRepository.Verify(r => r.SearchAsync(searchDto), Times.Once);
    }

    [Fact]
    public async Task SearchProductsAsync_WithMultipleFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        var searchDto = new ProductSearchDto
        {
            SearchTerm = "gaming laptop",
            CategoryId = 1,
            MinPrice = 500m,
            MaxPrice = 2000m,
            InStock = true,
            SortBy = "Price",
            SortOrder = "desc",
            PageNumber = 2,
            PageSize = 20
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = new List<Product>(),
            TotalCount = 0,
            PageNumber = 2,
            PageSize = 20
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        _mockProductRepository.Verify(r => r.SearchAsync(It.Is<ProductSearchDto>(dto =>
            dto.SearchTerm == "gaming laptop" &&
            dto.CategoryId == 1 &&
            dto.MinPrice == 500m &&
            dto.MaxPrice == 2000m &&
            dto.InStock == true &&
            dto.SortBy == "Price" &&
            dto.SortOrder == "desc" &&
            dto.PageNumber == 2 &&
            dto.PageSize == 20
        )), Times.Once);
    }

    [Fact]
    public async Task SearchProductsAsync_WithNoParameters_ShouldReturnAllActiveProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto();
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } },
            new Product { Id = 2, Name = "Product 2", Price = 20m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchProductsAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        var searchDto = new ProductSearchDto { CategoryId = 2 };
        var products = new List<Product>
        {
            new Product { Id = 3, Name = "Product 3", Price = 30m, IsActive = true, CategoryId = 2, Category = new Category { Id = 2, Name = "Cat2" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CategoryId.Should().Be(2);
    }

    [Fact]
    public async Task SearchProductsAsync_WithPriceRange_ShouldFilterByPriceRange()
    {
        // Arrange
        var searchDto = new ProductSearchDto { MinPrice = 50m, MaxPrice = 150m };
        var products = new List<Product>
        {
            new Product { Id = 4, Name = "Product 4", Price = 100m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Price.Should().Be(100m);
    }

    [Fact]
    public async Task SearchProductsAsync_WithInStockTrue_ShouldFilterInStockProducts()
    {
        // Arrange
        var searchDto = new ProductSearchDto { InStock = true };
        var products = new List<Product>
        {
            new Product { Id = 5, Name = "Product 5", Price = 50m, StockQuantity = 10, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.All(p => p.StockQuantity > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SearchProductsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var searchDto = new ProductSearchDto { PageNumber = 2, PageSize = 5 };
        var products = new List<Product>
        {
            new Product { Id = 6, Name = "Product 6", Price = 60m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 15,
            PageNumber = 2,
            PageSize = 5
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task SearchProductsAsync_WithSorting_ShouldSortResults()
    {
        // Arrange
        var searchDto = new ProductSearchDto { SortBy = "Price", SortOrder = "desc" };
        var products = new List<Product>
        {
            new Product { Id = 7, Name = "Product 7", Price = 200m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } },
            new Product { Id = 8, Name = "Product 8", Price = 100m, IsActive = true, CategoryId = 1, Category = new Category { Id = 1, Name = "Cat1" } }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProductsAsync_MapsProductDtosCorrectly()
    {
        // Arrange
        var searchDto = new ProductSearchDto();
        var products = new List<Product>
        {
            new Product
            {
                Id = 9,
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                CategoryId = 1,
                StockQuantity = 5,
                CreatedDate = new DateTime(2025, 1, 1),
                IsActive = true,
                Category = new Category { Id = 1, Name = "Test Category" }
            }
        };

        var pagedResult = new PagedResultDto<Product>
        {
            Items = products,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockProductRepository
            .Setup(r => r.SearchAsync(It.IsAny<ProductSearchDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchProductsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        var productDto = result.Items.First();
        productDto.Id.Should().Be(9);
        productDto.Name.Should().Be("Test Product");
        productDto.Description.Should().Be("Test Description");
        productDto.Price.Should().Be(99.99m);
        productDto.CategoryId.Should().Be(1);
        productDto.CategoryName.Should().Be("Test Category");
        productDto.StockQuantity.Should().Be(5);
        productDto.IsActive.Should().BeTrue();
    }

    #endregion
}