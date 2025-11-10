using ProductCatalog.Core.DTOs;

namespace ProductCatalog.Core.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllActiveProductsAsync();
    Task<ProductDto> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> SoftDeleteProductAsync(int id);
    Task<PagedResultDto<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
}