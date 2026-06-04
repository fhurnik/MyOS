# CLAUDE.md — MyOS Project Context

> Single source of truth for AI assistants (Claude, Copilot, Cursor, ChatGPT, etc.).
> Last verified and updated: 2026-06-04 (session: JWT auth + ICurrentUser implementation).

---

## Project Overview

**MyOS** is a production-grade **Modular Monolith Web API** built with **ASP.NET Core (.NET 10)**.

The system manages personal life domains: notes, learning, finance, fitness, and identity.

Architecture follows **pragmatic DDD**, **CQRS**, and **Clean Architecture** with clear separation between modules.

**Core principles:**
- Production quality, maintainable, testable code
- Simplicity over cleverness — avoid unnecessary abstractions
- Explicit logic, predictable behavior
- Clean Architecture layer boundaries are strictly enforced

---

## Solution Structure

```
MyOS/                                      ← repo root
├── CLAUDE.md
├── MyOS.slnx
├── Directory.Build.props                  ← global: net10.0, nullable, implicit usings
├── docker-compose.yml
├── .env / .env.example
├── init.sql / init-db.sh
│
├── MyOS.API/                              ← Web API entry point
├── MyOS.Migrator/                         ← Database migrations + SQL view sync
│
├── MyOS.Core.Domain/                      ← Shared base entities
├── MyOS.Core.Application/                 ← Shared CQRS contracts, Result, pagination, ICurrentUser, IUnitOfWork
├── MyOS.Core.Infrastructure/              ← EF Core setup, Serilog, DI extensions, CurrentUserService, UnitOfWork
│
├── MyOS.Identity.Domain/
├── MyOS.Identity.Application/
├── MyOS.Identity.Infrastructure/
│
└── src/                                   ← (reserved, currently empty/frontend)
```

**Modules planned but not yet implemented:** Notes, Learning, Finance, Fitness.
Each will follow the pattern: `MyOS.Modules.{Name}.Domain / Application / Infrastructure`.

> **Naming rule:** Use `MyOS.Modules.{ModuleName}.{Layer}` for future modules.
> Never use: `Shared`, `Common`, `Helpers`. Use `Core` for cross-cutting concerns.

---

## Layer Dependency Rules

```
Domain ← Application ← Infrastructure ← API
```

| Layer | Depends on | Must NOT depend on |
|---|---|---|
| Domain | nothing | EF Core, ASP.NET, SQL, DTOs, controllers |
| Application | Domain, Core.Application | EF Core, DbContext, infrastructure |
| Infrastructure | Application, Domain, Core.Infrastructure | Business logic |
| API | Infrastructure | DbContext, business logic, domain entities directly |

**Module isolation:** modules communicate only through public contracts. No tight coupling between modules.

---

## Architecture: Modular Monolith

Each module is an independent bounded context with its own:
- Domain model
- Application use cases
- Infrastructure (EF config, repositories)
- Database schema
- Migrations
- Controllers

**Module registration pattern** — every module exposes a single public DI method:

```csharp
public static IServiceCollection Add{ModuleName}Module(
    this IServiceCollection services,
    IConfiguration configuration)
```

This method registers: EF configurations, repositories, services, background services, authentication setup.

The API project never registers internal module services directly — it calls only `Add{ModuleName}Module(...)`.

**Current example** (`MyOS.Identity.Infrastructure/DependencyInjection.cs`):
```csharp
public static IServiceCollection AddIdentityModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddEfConfigurationsFromAssembly(typeof(UserEntityConfiguration).Assembly);
    services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => { /* TokenValidationParameters + JwtBearerEvents */ });
    services.AddIdentityApplication(); // registers MediatR + validators for this module
    return services;
}
```

**`AddIdentityApplication()`** — internal DI extension in each module's Application project:
```csharp
public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
    services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    return services;
}
```

**MediatR is registered per-module** (not globally). `AddCoreApplication()` only adds `ValidationBehavior`.

**Core registration** (`MyOS.Core.Infrastructure/Extensions/DependencyInjection.cs`):
```csharp
public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
{
    services.AddCoreApplication();        // ValidationBehavior pipeline
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("Database")));
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddHttpContextAccessor();
    services.AddScoped<ICurrentUser, CurrentUserService>();
    return services;
}
```

---

## CQRS

Commands and queries are strictly separated.

| Side | Technology | Purpose |
|---|---|---|
| Command | EF Core + Domain | Write operations, transactions, domain logic |
| Query | SQLKata + SQL Views | Read operations, DTO projections |

**Command interfaces** (`MyOS.Core.Application/Abstractions/Messaging/`):
```csharp
public interface ICommand<T> : IRequest<Result<T>>;
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
```

**Query interfaces:**
```csharp
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
public interface IQueryHandler<Query, TResponse> : IRequestHandler<Query, Result<TResponse>>
    where Query : IQuery<TResponse>;
```

**Rules:**
- Queries **MUST** use SQLKata. Never EF Core or DbContext in query handlers.
- Commands use EF Core + Domain entities + `IUnitOfWork.SaveChangesAsync` (once per handler).
- Never call `SaveChanges` more than once per command handler.
- All command handlers inject `IUnitOfWork` and call it at the end.

**MediatR** is the mediator. Registered per-module via `AddIdentityApplication()` pattern.

---

## Command File Convention (Slice)

Each feature = one folder with one `*Command.cs` file (containing all three classes) and optionally a sibling DTO file.

```
Commands/
├── Register/
│   └── RegisterCommand.cs        ← RegisterCommand + RegisterCommandValidator + RegisterCommandHandler
├── Login/
│   ├── LoginCommand.cs           ← LoginCommand + LoginCommandValidator + LoginCommandHandler
│   └── (reuses Shared/AuthTokens.cs)
├── RefreshToken/
│   └── RefreshTokenCommand.cs    ← RefreshTokenCommand + RefreshTokenCommandValidator + RefreshTokenCommandHandler
└── Shared/
    └── AuthTokens.cs             ← shared response DTO
```

**Order within the file:** Command → Validator → Handler (matches the flow of a request).

**Response DTO naming:**
- Command results: descriptive noun describing the data, not the action — e.g., `AuthTokens` (not `LoginResponse`)
- When multiple commands return the same data, use a shared DTO in `Commands/Shared/`
- Query results: `{Entity}Dto` suffix — e.g., `UserDto`, `NoteDto`

---

## Error Codes — Static Error Classes

Never hardcode error codes as inline strings in handlers. Use static error classes.

**Pattern:**
```csharp
// MyOS.Identity.Application/Errors/UserErrors.cs
public static class UserErrors
{
    public static readonly Error EmailAlreadyInUse =
        Error.Conflict("User.EmailAlreadyInUse", "A user with this email is already registered.");
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");
    // ...
}
```

**Location:** `{Module}.Application/Errors/{Entity}Errors.cs`
— in **Application** layer (not Domain), because `Error` type is from `Core.Application`.

**In handlers:**
```csharp
return Result<Guid>.Failure(UserErrors.EmailAlreadyInUse);   // not: Error.Conflict("User.EmailAlreadyInUse", "...")
```

---

## Cross-cutting Core Abstractions

### IUnitOfWork

Defined in `MyOS.Core.Application/Abstractions/IUnitOfWork.cs`. Implemented in `MyOS.Core.Infrastructure/Persistence/UnitOfWork.cs`. Registered in `AddCore()`.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

Every command handler injects `IUnitOfWork` and calls it once at the end. **Do not define `IUnitOfWork` per-module** — it lives in Core and is shared.

### ICurrentUser

Defined in `MyOS.Core.Application/Abstractions/ICurrentUser.cs`. Implemented in `MyOS.Core.Infrastructure/Services/CurrentUserService.cs`. Registered in `AddCore()`.

```csharp
public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
```

- Any module's handler can inject `ICurrentUser` to get caller context (ownership checks, audit, data scoping).
- `Id` and `Email` throw `InvalidOperationException` if accessed when `IsAuthenticated == false` — accessing them on an `[Authorize]` endpoint is always safe.
- Check `IsAuthenticated` first on endpoints that allow anonymous access.

---

## Result Pattern

All predictable errors use `Result<T>` — never throw exceptions for business flow.

**Implementation** (`MyOS.Core.Application/Abstractions/Results/`):

```csharp
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public T Value { get; } // throws if IsFailure

    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
}
```

```csharp
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None;
    public static Error Validation(string code, string message);
    public static Error NotFound(string code, string message);
    public static Error Conflict(string code, string message);
    public static Error Unauthorized(string code, string message);
    public static Error Forbidden(string code, string message);
    public static Error Failure(string code, string message);
    public static Error Unexpected(string code, string message);
}
```

```csharp
public enum ErrorType { None, Validation, NotFound, Conflict, Unauthorized, Forbidden, Failure, Unexpected }
```

**Use Result for:** validation errors, not found, conflicts, authorization failures, domain rule violations.

**Throw exceptions only for:** system failures, infrastructure failures, unexpected errors, programming errors (wrong API usage).

---

## API Layer

**File:** `MyOS.API/`

- **Controllers only** — no Minimal API
- All controllers inherit `ApiControllerBase`
- REST conventions with API versioning

```csharp
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
        => result.ToActionResult(HttpContext);
}
```

**Result → HTTP mapping** (`ResultExtensions.ToActionResult`):

| ErrorType | HTTP Status |
|---|---|
| Validation | 400 Bad Request |
| NotFound | 404 Not Found |
| Conflict | 409 Conflict |
| Unauthorized | 401 Unauthorized |
| Forbidden | 403 Forbidden |
| Failure / Unexpected | 500 Internal Server Error |
| Success (Unit) | 204 No Content |
| Success (T) | 200 OK |

**ProblemDetails response** always includes:
- `status`, `title`, `detail`, `instance`
- `traceId` (from `Activity.Current?.Id`)
- `correlationId` (from `HttpContext.TraceIdentifier`)
- `errorCode` (from `Error.Code`)

**Route conventions:**
```
GET    /api/v1/{resource}
GET    /api/v1/{resource}/{id}
POST   /api/v1/{resource}
PUT    /api/v1/{resource}/{id}
DELETE /api/v1/{resource}/{id}
```

**API versioning:** `Asp.Versioning.Mvc` with `ApiVersionReader.Combine(UrlSegmentApiVersionReader, HeaderApiVersionReader("api-version"))`. Clients can specify version via URL (`/api/v1/`) or header (`api-version: 1.0`).

**Authorization:** `[Authorize]` added per-controller (not globally). Public endpoints (auth, health) use `[AllowAnonymous]`. `AuthController` has `[AllowAnonymous]` at the controller level.

**Rules:**
- Always use API versioning (`[ApiVersion("1.0")]` + route with `{version:apiVersion}`)
- Always validate input (FluentValidation via MediatR pipeline)
- Always use DTOs — never return domain entities directly
- Always use `CancellationToken` in controller actions
- Never access DbContext from controllers
- Never put business logic in controllers

---

## Auth Failure Format

JWT auth failures return `ProblemDetails` in our standard format (not ASP.NET's default bare 401/403).

Implemented via `JwtBearerEvents` in `Identity.Infrastructure/DependencyInjection.cs`:
- `OnChallenge` — missing or invalid token → 401 with `UserErrors.Unauthorized`
- `OnForbidden` — valid token, insufficient permissions → 403 with `UserErrors.Forbidden`

**Do not** add a custom middleware for JWT validation — `UseAuthentication()` handles it. Use `JwtBearerEvents` for format customization only.

---

## Global Exception Handling

`GlobalExceptionHandlingMiddleware` catches all unhandled exceptions.

Returns `ProblemDetails` with status 500 — never exposes stack traces.

Logs with structured logging:
```csharp
logger.LogError(exception,
    "Unhandled exception occurred while processing request {Method} {Path}",
    context.Request.Method, context.Request.Path);
```

---

## Validation

FluentValidation via MediatR pipeline behavior (`ValidationBehavior<TRequest, TResponse>`).

- Runs all `IValidator<TRequest>` implementations registered in DI
- On failure: returns `Result<T>.Failure(Error.Validation("Validation.Failed", message))`
- Multiple validation errors are joined with `Environment.NewLine`
- No exception is thrown — the pipeline short-circuits with a Result failure

**Register validators** via `services.AddValidatorsFromAssembly(...)` in the module's Application DI extension.

**Validator placement:** in the same file as the Command it validates (slice convention).

---

## Domain Layer

**Base entity** (`MyOS.Core.Domain/Entities/Entity.cs`):
```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
}
```

**Entity rules:**
- Constructors are `internal` — use static factory methods (`Create(...)`)
- Properties have `private set`
- Mutating methods are `internal`
- Private parameterless constructor for EF Core (initialize non-nullable strings with `null!`)
- Timestamps: `CreatedAtUtc` (DateTime), `UpdatedAtUtc` (DateTime?) — add per entity as needed
- Soft delete (`DeletedAtUtc`) is a global convention but must be added explicitly per entity — do not add it unless that entity's lifecycle requires it

**Example — `User` entity:**
```csharp
public class User : Entity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static User Create(string firstName, string lastName, string email, string passwordHash);
    internal void Update(string firstName, string lastName);
    internal void ChangePassword(string newPasswordHash);
    internal void ChangeActiveStatus(bool isActive);
    private User() { FirstName = null!; LastName = null!; Email = null!; PasswordHash = null!; }
}
```

**Domain layer contains:**
- Entities, Aggregates, Value Objects
- Domain Events
- Business Rules
- Domain Exceptions
- Enums
- Domain Services (only when logic doesn't belong to an entity)
- Repository interfaces (`IUserRepository`)

**Domain layer must NOT contain:** DTOs, error codes (those go in Application), controllers, logging, database code, EF Core references.

---

## Infrastructure Layer

**EF Core setup** — single shared `AppDbContext` that loads configurations from all registered modules:

```csharp
public sealed class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var assembly in _efModuleOptions.ConfigurationAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
```

Modules register their EF configurations via:
```csharp
services.AddEfConfigurationsFromAssembly(typeof(SomeEntityConfiguration).Assembly);
```

**Entity configuration base class:**
```csharp
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
```

**Always map all columns explicitly** in entity configurations — do not rely on EF Core naming conventions:
```csharp
internal class UserEntityConfiguration : EntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
        // ... all columns explicitly mapped
    }
}
```

**Infrastructure contains:** EF Core, SQLKata, repositories, external services, background services, logging config, Serilog.

**Infrastructure must NOT contain** business logic.

---

## Database

- **SQL Server 2022** (Docker: `mcr.microsoft.com/mssql/server:2022-latest`)
- Each module has its own schema:

| Module | Schema |
|---|---|
| Identity | `identity` |
| Notes | `notes` (planned) |
| Learning | `learning` (planned) |
| Finance | `finance` (planned) |
| Fitness | `fitness` (planned) |
| System | `system` |

**Soft delete:** use `deleted_at_utc` column when a specific entity's lifecycle requires it. Not every entity needs soft delete by default — add it explicitly when needed.

**Column naming in database:** `snake_case` (e.g., `created_at_utc`, `password_hash`).

---

## Migrations

Migrations use **FluentMigrator** — never EF Core migrations.

**Project:** `MyOS.Migrator/`

**Directory structure:**
```
MyOS.Migrator/
├── Migrations/
│   ├── system/         ← system-level tables
│   └── Identity/       ← identity module migrations
│       └── (Notes/, Learning/, Finance/, Fitness/ — planned)
└── Views/
    └── Identity/
        └── v_users.sql
```

**Migration naming:**
```
YYYYMMDDHHNN_ActionDescription.cs
```
Examples:
```
Migrations/Identity/202604261255_CreateIdentitySchema.cs
Migrations/Identity/202606041200_AddMissingUserColumns.cs
```
Migration class attribute: `[Migration(YYYYMMDDHHNN)]` — globally unique numeric timestamp.

**Rules:**
- Never rename existing migration files
- Never modify already-executed migrations
- Create a new migration for any change

**Migration class pattern:**
```csharp
[Migration(202606041200)]
public sealed class AddMissingUserColumns : Migration
{
    public override void Up() { /* FluentMigrator fluent API */ }
    public override void Down() { /* rollback */ }
}
```

---

## SQL Views

SQL views serve as **read models** for the Query side of CQRS.

**Location:** `MyOS.Migrator/Views/{Module}/v_{name}.sql`

**Synchronization:** `SqlViewSynchronizer` runs after migrations and applies `.sql` files from `Views/` directory. It uses SHA256 hashing tracked in `system.sql_file_history` — only changed files are re-applied.

**View pattern:** `CREATE OR ALTER VIEW [{Schema}].[v_{name}]`

```sql
-- Views/Identity/v_users.sql
CREATE OR ALTER VIEW [Identity].[v_users]
AS
SELECT id, email, created_at_utc
FROM [identity].[users];
```

**Rules:**
- Views expose only required columns
- Queries use views via SQLKata, not raw EF Core

---

## Security

- JWT authentication — **implemented** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Refresh tokens — **implemented** (stateful, stored in `identity.refresh_tokens` table)
- `JwtSettings` options class in Identity.Application — `SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiryMinutes`, `RefreshTokenExpiryDays`
- `SecretKey` must be stored in User Secrets for development, never in `appsettings.json`
- Always validate input
- Always check user ownership (use `ICurrentUser.Id` to scope queries)
- Never store passwords in plain text — BCrypt with work factor 12
- Never trust user input
- Never log passwords, refresh tokens, JWT tokens, API keys, personal sensitive data

---

## Observability (Planned)

- OpenTelemetry for distributed tracing
- Health checks at `/health/live` and `/health/ready`
- Metrics
- Correlation ID propagation

Currently implemented: Serilog structured logging, `traceId` and `correlationId` in all `ProblemDetails` responses (including auth failures).

---

## Pagination

`PagingRequest` and `PagingList<T>` are in `MyOS.Core.Application/Abstractions/Pagination/`.

```csharp
public class PagingRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize > 100 ? 100 : PageSize; // max 100
}
```

---

## Logging

**Serilog** — configuration in `MyOS.Core.Infrastructure/Logging/SerilogConfiguration.cs`.

**Structured logging — always use message templates:**
```csharp
// Correct
_logger.Information("Note {NoteId} created by user {UserId}", noteId, userId);

// Wrong — never use string interpolation
_logger.Information($"Note {noteId} created by user {userId}");
```

**Always log:** application startup/shutdown, unhandled exceptions, warnings, important domain events, security-related events.

**Include when available:** `correlationId`, `traceId`, `userId`, `requestPath`, `moduleName`.

**Never log:** passwords, refresh tokens, JWT tokens, API keys, personal sensitive data.

---

## Dependency Injection Rules

| Lifetime | Use for |
|---|---|
| Scoped | Business services, repositories, handlers |
| Transient | Lightweight, stateless components |
| Singleton | Only when explicitly safe (e.g., cached config) |

Never use static services.

---

## Docker

`docker-compose.yml` defines two services:

| Service | Description |
|---|---|
| `myos-sqlserver` | SQL Server 2022 Developer edition |
| `myos-migrator` | Runs migrations + view sync on startup |

Database is initialized via `init-db.sh` / `init.sql` (creates the database if it doesn't exist).

Migrator waits for SQL Server health check before running.

Connection string env var: `ConnectionStrings__Database`

---

## Global Build Configuration

`Directory.Build.props` applies to all projects:

```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<LangVersion>latest</LangVersion>
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

---

## Key NuGet Packages

| Package | Version | Used in |
|---|---|---|
| MediatR | 14.1.0 | Core.Application |
| FluentValidation | 12.1.1 | Core.Application, Identity.Application |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Identity.Application |
| Microsoft.Extensions.Options | 10.0.7 | Identity.Application |
| EF Core (SqlServer) | 10.0.8 | Core.Infrastructure |
| Serilog | 4.3.1 | Core.Infrastructure |
| Serilog.Extensions.Hosting | 10.0.0 | Core.Infrastructure |
| Serilog.Sinks.Console | 6.1.1 | Core.Infrastructure |
| FrameworkReference: Microsoft.AspNetCore.App | — | Core.Infrastructure (for IHttpContextAccessor) |
| BCrypt.Net-Next | 4.0.3 | Identity.Infrastructure |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.6 | Identity.Infrastructure |
| Asp.Versioning.Mvc | 8.1.0 | MyOS.API |
| Microsoft.AspNetCore.OpenApi | 10.0.6 | MyOS.API |
| FluentMigrator | 8.0.1 | MyOS.Migrator |

**SQLKata** — referenced in instructions for query-side reads but **not yet in any csproj**. Add to module Infrastructure when implementing the first Query handler.

---

## Testing (Planned)

| Type | Tool | Purpose |
|---|---|---|
| Unit | xUnit | Domain logic |
| Integration | xUnit + Testcontainers | API endpoints |
| Architecture | ArchUnitNET or NetArchTest | Layer boundary enforcement |

Rules:
- Tests must be deterministic, isolated, not dependent on local machine state
- Integration tests use Testcontainers (real SQL Server in Docker)
- No mocking the database in integration tests

---

## Things AI Must Never Do

- Put business logic in controllers
- Use DbContext or EF Core in query handlers
- Create unnecessary abstractions or interfaces for everything
- Create static state
- Bypass validation
- Mix layers
- Physically delete records when soft delete applies to that entity
- Create tight coupling between modules
- Call `SaveChanges` more than once per command handler
- Start transactions in controllers or repositories
- Use string interpolation in log messages
- Return domain entities directly from API
- Use Minimal API (use Controllers)
- Use EF Core migrations (use FluentMigrator)
- Hardcode error codes/messages as inline strings — use static error classes (`UserErrors.*`)
- Define `IUnitOfWork` per-module — it lives in `Core.Application` and is shared
- Add a custom JWT validation middleware — use `JwtBearerEvents` for auth failure format
- Add soft delete (`DeletedAtUtc`) to an entity unless that entity's lifecycle specifically requires it

---

## Adding a New Module — Checklist

1. Create projects: `MyOS.Modules.{Name}.Domain`, `MyOS.Modules.{Name}.Application`, `MyOS.Modules.{Name}.Infrastructure`
2. Add schema migration: `Migrations/{Name}/YYYYMMDDHHNN_Create{Name}Schema.cs`
3. Add table migrations per entity
4. Add SQL views in `Views/{Name}/v_{name}.sql`
5. Implement domain entities extending `Entity`, repository interfaces in Domain
6. Create `{Entity}Errors.cs` in `{Name}.Application/Errors/` for static error definitions
7. Implement commands (slice: command + validator + handler in one file) and queries with handlers
8. Add EF entity configurations extending `EntityConfiguration<T>` with full explicit column mapping
9. Add `Add{Name}Application()` DI extension in Application (registers MediatR + validators for this assembly)
10. Expose `Add{Name}Module(IServiceCollection, IConfiguration)` from Infrastructure DI — registers EF configs, repos, application DI
11. Register in `Program.cs` via `builder.Services.Add{Name}Module(builder.Configuration)`
12. Add controllers in `MyOS.API/` extending `ApiControllerBase`, with `[ApiVersion("1.0")]` and `[Authorize]` where needed

---

## Discrepancy Notes

- `copilot-instructions.md` shows solution path as `src/MyOS.Api`, but the actual project is `MyOS.API/` at repo root (no `src/` folder in use)
- `copilot-instructions.md` mentions `MyOS.Modules.*` naming — current Identity module uses `MyOS.Identity.*` (without `Modules` segment). Future modules should follow `MyOS.Modules.{Name}.*`
- System migration `20260426_CreateSqlFileHistoryTable.cs` uses `[Migration(2026042601)]` format (no time component) — standardize on full `YYYYMMDDHHNN` format for all new migrations
- OpenTelemetry, health checks, SQLKata, test projects — **planned, not implemented**
