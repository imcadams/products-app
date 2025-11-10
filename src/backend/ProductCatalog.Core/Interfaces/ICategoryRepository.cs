using ProductCatalog.Core.Entities;

namespace ProductCatalog.Core.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}