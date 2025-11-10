using Microsoft.EntityFrameworkCore;
using ProductCatalog.Core.Entities;
using ProductCatalog.Core.Interfaces;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ProductCatalogDbContext _context;

    public CategoryRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllActiveAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products.Where(p => p.IsActive))
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products.Where(p => p.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        category.IsActive = true;
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await SoftDeleteAsync(id);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null || !category.IsActive)
            return false;

        // Check if there are active products in this category
        var hasActiveProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id && p.IsActive);
        
        if (hasActiveProducts)
        {
            throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it contains active products");
        }

        category.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && c.IsActive);
    }
}