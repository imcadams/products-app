# GitHub Copilot Instructions

## Project Context
- This repository contains a C# .NET 8 Web API for a Product Catalog system and a minimal Angular frontend for browsing catalog data
- Uses SQL Server Express LocalDB and Entity Framework Core
- Applies Clean Architecture with layers for API (presentation), Core (application and domain), Infrastructure (data/repository)

## Technologies
- .NET 8, ASP.NET Core Web API
- Entity Framework Core
- SQL Server Express LocalDB
- Angular (frontend)
- xUnit (unit tests)

## Project Structure
- API Project: Controllers, middleware, filters
- Core Project: Entities, DTOs, interfaces, services
- Infrastructure Project: DbContext, repositories, config
- Angular frontend, minimal implementation

## Coding Standards
- Use Repository Pattern for data access abstraction
- Prefer async methods throughout
- DTOs exposed to API, entities internal
- Use Dependency Injection (constructor-based)
- Use explicit validation with DataAnnotations
- Employ clear naming conventions (PascalCase for classes, camelCase for fields)
- Use `.AsNoTracking()` for all read-only Entity Framework queries to improve performance
- Avoid N+1 queries by using eager loading (`.Include()`) for navigation properties that will be accessed

## Behavioral Guidelines
- Do not add authentication until explicitly requested
- Add [Authorize] comments/placeholders at controller level as a signal for future work, e.g. `// [Authorize]` or interface markers
- Document validation logic, error responses, testing strategy
- Avoid modifying tests or build scripts unless requested
- Always update README.md when architectural or command changes are made

## Build/Run Instructions
- Backend: LocalDB with migrations, use `dotnet run`
- Frontend: `npm install && npm start`
- Tests: Run with `dotnet test`

## Additional Instructions
- Focus code generation on the .NET 8 API and supporting layers
- Use clear, maintainable patterns for repository and service layers
- Make solutions credibly extensible (e.g. for auth, advanced search)
- Reference Software Design Document (SDD) for architecture

# End of Copilot Instructions
