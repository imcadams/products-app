# Software Design Document (SDD)

## Product Catalog API - Technical Assessment

**Version:** 1.0  
**Date:** November 9, 2025  
**Target Framework:** .NET 8  
**Database:** SQL Server Express LocalDB

---

## 1. Executive Summary

This document details the architecture for a Product Catalog .NET 8 Web API, implementing clean architecture principles, Repository Pattern, and Entity Framework Core, optimized for technical assessment evaluation.

---

## 2. System Architecture

### 2.1 Project Structure

- **src/backend/ProductCatalog.API**: Web API, controllers, startup, filters, DI.
- **src/backend/ProductCatalog.Core**: Entities, DTOs, interfaces, services.
- **src/backend/ProductCatalog.Infrastructure**: Data access (EFCore), repositories.
- **src/frontend/product-catalog-app**: Angular (minimal implementation).

### 2.2 Architectural Diagram

```
+--------------------+
| Presentation Layer |
|    (API)           |
+---------+----------+
          |
+---------v----------+
| Application Layer  |
|    (Core)          |
+---------+----------+
          |
+---------v----------+
| Infrastructure     |
|    (Infrastructure)|
+--------------------+
```

---

## 3. Database Design

### 3.1 ER Diagram

```
Category    <1---N>   Product
```

### 3.2 Schema

- **Product**: Id, Name, Description, Price, StockQuantity, CreatedDate, CategoryId (FK), IsActive
- **Category**: Id, Name, Description, IsActive

**Indexes:**
- Primary Keys and Foreign Keys (automatic)
- `IX_Products_CategoryId_IsActive` - Composite index for category filtering with active status
- `IX_Products_IsActive_Name` - Composite index for active products with name sorting
- `IX_Products_IsActive_Price` - Supports price range filtering and price sorting
- `IX_Products_IsActive_CreatedDate` - Supports creation date sorting
- `IX_Products_IsActive_StockQuantity` - Supports stock availability filtering (InStock parameter)
- `IX_Products_CategoryId_IsActive_Price` - Composite index for combined category + price filters

**Index Design Rationale:**
- All indexes include `IsActive` as the leading column since search queries always filter by active status
- Composite indexes support multiple filter combinations commonly used together
- Separate indexes for different sorting options (Price, CreatedDate, Name)
- Text search on Name/Description benefits from `IX_Products_IsActive_Name` index

---

## 4. API Endpoints Overview

- `GET /api/products`: List all active products with category info
- `GET /api/products/{id}`: Get single active product
- `POST /api/products`: Add product
- `PUT /api/products/{id}`: Update product
- `DELETE /api/products/{id}`: Soft-delete (IsActive = false)
- `GET /api/categories`: List categories
- `POST /api/categories`: Add category
- `GET /api/products/search`: Search products (all filters + pagination)

---

## 5. Component and Code Design

### 5.1 Core Entities
```csharp
// Product.cs
public class Product {
    public int Id {get;set;}
    public string Name {get;set;} = string.Empty;
    public string? Description {get;set;}
    public decimal Price {get;set;}
    public int CategoryId {get;set;}
    public int StockQuantity {get;set;}
    public DateTime CreatedDate {get;set;}
    public bool IsActive {get;set;} = true;
    public Category Category {get;set;} = null!;
}

// Category.cs
public class Category {
    public int Id {get;set;}
    public string Name {get;set;} = string.Empty;
    public string? Description {get;set;}
    public bool IsActive {get;set;} = true;
    public ICollection<Product> Products {get;set;} = new List<Product>();
}
```

### 5.2 Repository & Service Interfaces
```csharp
public interface IProductRepository {
    Task<IEnumerable<Product>> GetAllActiveAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(int id);
    Task<PagedResultDto<Product>> SearchAsync(ProductSearchDto searchDto);
}
```

---

## 6. Validation & Error Handling
- DataAnnotations on DTOs
- 404 on missing resources
- Global exception handling middleware

---

## 7. Security Considerations
- Placeholder `[Authorize]` for controllers
- Auth interfaces, but not implemented (see README)
- Document as a future enhancement

---

## 8. Testing
- xUnit test project for Core and Infrastructure
- Mock repositories for services

---

## 9. Build & Run
- LocalDB + migrations, `dotnet run` for backend
- `npm install && npm start` for frontend

---

## 10. Presentation/README Guidance
- Focus on rationale for architectural choices
- Document endpoints, search, testing strategy, how to extend
- Note intentional placeholders for auth

---

# End of SDD
