using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Core.DTOs;

public class CreateProductDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Valid Category ID is required")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int StockQuantity { get; set; }
}