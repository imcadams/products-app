using ProductCatalog.Core.Entities;
using ProductCatalog.Core.DTOs;

namespace ProductCatalog.Core.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllActiveAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(int id);
    Task<PagedResultDto<Product>> SearchAsync(ProductSearchDto searchDto);
    Task<bool> ExistsAsync(int id);
}