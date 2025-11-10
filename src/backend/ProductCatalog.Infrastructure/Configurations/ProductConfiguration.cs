using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Core.Entities;

namespace ProductCatalog.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign Keys
        builder.Property(p => p.CategoryId)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for optimal search performance
        builder.HasIndex(p => new { p.CategoryId, p.IsActive })
            .HasDatabaseName("IX_Products_CategoryId_IsActive");

        builder.HasIndex(p => new { p.IsActive, p.Name })
            .HasDatabaseName("IX_Products_IsActive_Name");
        
        // Supports sorting by price and price range filters
        builder.HasIndex(p => new { p.IsActive, p.Price })
            .HasDatabaseName("IX_Products_IsActive_Price");

        // Supports sorting by creation date
        builder.HasIndex(p => new { p.IsActive, p.CreatedDate })
            .HasDatabaseName("IX_Products_IsActive_CreatedDate");

        // Supports stock availability filtering (InStock parameter)
        builder.HasIndex(p => new { p.IsActive, p.StockQuantity })
            .HasDatabaseName("IX_Products_IsActive_StockQuantity");

        // Composite index for combined category + price filters (common search pattern)
        builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.Price })
            .HasDatabaseName("IX_Products_CategoryId_IsActive_Price");

        // Table name
        builder.ToTable("Products");
    }
}