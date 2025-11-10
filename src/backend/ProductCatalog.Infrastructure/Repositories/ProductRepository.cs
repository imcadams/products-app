using Microsoft.EntityFrameworkCore;
using ProductCatalog.Core.DTOs;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductCatalogDbContext _context;

    public ProductRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllActiveAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedDate = DateTime.UtcNow;
        product.IsActive = true;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Load the category for the created product
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();
        
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        
        // Load the category for the updated product
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();
        
        return product;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return false;

        product.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResultDto<Product>> SearchAsync(ProductSearchDto searchDto)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Apply search filter - case-insensitive, searches both name and description
        // Multi-word search with AND logic
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var searchTerms = searchDto.SearchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in searchTerms)
            {
                var lowerTerm = term.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(lowerTerm) || 
                    (p.Description != null && p.Description.ToLower().Contains(lowerTerm))
                );
            }
        }

        // Apply category filter
        if (searchDto.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);
        }

        // Apply price filters
        if (searchDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
        }

        if (searchDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
        }

        // Apply stock filter
        if (searchDto.InStock.HasValue && searchDto.InStock.Value)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Apply sorting
        var sortOrder = searchDto.SortOrder?.ToLowerInvariant();
        query = searchDto.SortBy?.ToLowerInvariant() switch
        {
            "name" => sortOrder == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => sortOrder == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "created" => sortOrder == "desc" ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate),
            _ => query.OrderBy(p => p.Name)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        return new PagedResultDto<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize
        };
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == id && p.IsActive);
    }
}