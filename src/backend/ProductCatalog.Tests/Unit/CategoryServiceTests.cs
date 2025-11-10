using FluentAssertions;
using Moq;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Exceptions;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Core.Services;

namespace ProductCatalog.Tests.Unit;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _service = new CategoryService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllActiveCategoriesAsync_ShouldReturnMappedCategoryDtos()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category
            {
                Id = 1,
                Name = "Electronics",
                Description = "Electronic devices",
                IsActive = true
            },
            new Category
            {
                Id = 2,
                Name = "Books",
                Description = "All kinds of books",
                IsActive = true
            }
        };

        _mockRepository
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllActiveCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Electronics");
        result.Should().Contain(c => c.Name == "Books");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithValidId_ShouldReturnCategoryDto()
    {
        // Arrange
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices",
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Electronics");
        result.Description.Should().Be("Electronic devices");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await _service.Invoking(s => s.GetCategoryByIdAsync(999))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category with ID 999 was not found");
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidDto_ShouldCreateAndReturnCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "New Category",
            Description = "New category description"
        };

        var createdCategory = new Category
        {
            Id = 3,
            Name = createDto.Name,
            Description = createDto.Description,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _service.CreateCategoryAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Category");
        result.Description.Should().Be("New category description");
        result.IsActive.Should().BeTrue();

        _mockRepository.Verify(r => r.CreateAsync(It.Is<Category>(c => 
            c.Name == createDto.Name && 
            c.Description == createDto.Description)), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidDto_ShouldUpdateAndReturnCategory()
    {
        // Arrange
        var existingCategory = new Category
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old description",
            IsActive = true
        };

        var updateDto = new CreateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        var updatedCategory = new Category
        {
            Id = 1,
            Name = updateDto.Name,
            Description = updateDto.Description,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingCategory);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .ReturnsAsync(updatedCategory);

        // Act
        var result = await _service.UpdateCategoryAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated description");
        result.IsActive.Should().BeTrue();

        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Category>(c => 
            c.Name == updateDto.Name && 
            c.Description == updateDto.Description)), Times.Once);
    }
}