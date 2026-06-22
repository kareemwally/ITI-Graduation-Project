# Fayed API

Backend (ASP.NET Core Web API) for **Fayed** — a B2B marketplace for industrial surplus.
The solution implements a classic **3-layer (N-tier) architecture** with Dependency
Injection / IoC and SOLID principles throughout.

## Architecture

```
Fayed-API  (Presentation / API)   →   BLL  (Business Logic)   →   DAL  (Data Access)
  Controllers                          Managers (services)          EF Core DbContext
  Program.cs (composition root)        DTOs + Validators            Entities (ERD)
  Swagger / OpenAPI                    Mapping                      Generic Repository
                                       ServiceExtension             Unit of Work
                                                                    ServiceExtension
```

Dependencies flow **inward only**: `API → BLL → DAL`. The API never references EF Core
entities directly — it talks to the BLL through DTOs and manager interfaces.

### Layers

| Project | Responsibility | Key building blocks |
|---------|----------------|---------------------|
| **DAL** | Persistence & domain model | `Models/` (22 entities + enums), `Data/FayedDbContext`, `Data/Configurations/` (one `IEntityTypeConfiguration` per entity), `Repos/` (generic repository), `UnitOfWork/`, `ServiceExtension/AddDataAccessLayer` |
| **BLL** | Business rules & orchestration | `Managers/` (services behind interfaces), `DTOs/`, `Validators/` (FluentValidation), `Mapping/`, `ServiceExtension/AddBusinessLogicLayer` |
| **Fayed-API** | HTTP surface | `Controllers/`, `Program.cs`, OpenAPI |

### How SOLID / DI / IoC are applied

- **SRP** – every entity has its own configuration class; every manager has a single
  area of responsibility; mapping lives in dedicated classes.
- **OCP** – the generic repository is registered open-generically
  (`IGenericRepository<>` → `GenericRepository<>`), so new entities need no new DI code.
- **LSP** – `GenericRepository<T>` members are `virtual`; specialised repositories can
  override behaviour without breaking callers.
- **ISP** – managers expose small, focused interfaces (`ICategoryManager`,
  `IListingManager`).
- **DIP** – managers depend on `IUnitOfWork` / `IGenericRepository<T>` abstractions,
  never on `FayedDbContext` directly.
- **IoC** – each layer ships a `ServiceExtension` that registers its own services.
  The API composes everything with a single `AddBusinessLogicLayer(...)` call, which
  in turn calls `AddDataAccessLayer(...)`. All dependencies are constructor-injected.

## Domain model (ERD v2.0 — 22 entities)

Auth & Users (`User`, `Role`, `UserRole`) · Geography (`Governorate`, `City`) ·
Factory & Verification (`Factory`, `Document`, `VerificationCase`) ·
AI (`AIVerificationResult`, `AISearchLog`) ·
Catalog (`Category`, `Listing`, `ListingMedia`, `SavedListing`) ·
Communication (`Chat`, `Message`) ·
Orders & Finance (`Order`, `Wallet`, `Transaction`, `Review`) ·
System (`Notification`, `AuditLog`).

Highlights faithfully mapped from the ERD:
- Enums stored as readable strings (`HasConversion<string>()`).
- Money `decimal(18,2)`, quantities `decimal(18,3)`, commission rate `decimal(5,4)`,
  AI confidence `decimal(4,3)`.
- Unique keys: `User.Email`, `Factory.CommercialRegistryNo` / `TaxCardNo` / `UserId`,
  `Wallet.UserId`, `AIVerificationResult.VerificationCaseId`, `Chat(ListingId,BuyerId)`,
  `Review(OrderId,ReviewerId)`.
- Check constraints: `Document(FactoryId OR OrderId)`, `Review.Rating BETWEEN 1 AND 5`.
- Soft delete (`IsDeleted`/`DeletedAt`) on the critical tables (`User`, `Factory`,
  `Listing`, `Order`) via a global query filter + `SaveChanges` interception.
- Seed data: 5 roles, 27 governorates, a starter category tree.

## Getting started

Prerequisites: **.NET 9 SDK**, **SQL Server** (LocalDB / Express works).

```bash
# 1. Configure the connection string in Fayed-API/appsettings.json (DefaultConnection)

# 2. Restore & build
dotnet build Fayed-API.slnx

# 3. Create the database from the model (migrations live in the DAL project)
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project DAL --startup-project Fayed-API
dotnet ef database update --project DAL --startup-project Fayed-API

# 4. Run the API
dotnet run --project Fayed-API
```

OpenAPI is exposed at `/openapi/v1.json` in Development.

### Migration order (from the ERD doc)

The model is designed so a single `InitialCreate` migration works, but if you prefer the
documented incremental order it is: Auth & Roles → Geography → Factories & Categories →
Factory Documents → Verification Cases → Listings → Chats → Orders → Order Documents →
Wallets → Reviews → AI Layer → System.

## Implemented vertical slices

`Categories` and `Listings` are wired end-to-end (Controller → Manager → Validator →
Mapping → Repository/UoW → DbContext) as the reference pattern. The remaining entities
already have their model + EF configuration; adding their managers/controllers follows
the exact same pattern.
