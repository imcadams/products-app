using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Core.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    public int StockQuantity { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property
    public Category Category { get; set; } = null!;
}