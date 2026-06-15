# CLAUDE.md — MyOS Project Context

> Single source of truth for AI assistants (Claude, Copilot, Cursor, ChatGPT, etc.).

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
- Hardcode error codes as inline strings in handlers — use static error classes (`UserErrors.*`)
- Put error messages inline in error code classes — messages go in `.resx` resource files only
- Use magic strings for resource manifest names — derive from `typeof().Namespace` + language code map
- Define `IUnitOfWork` per-module — it lives in `Core.Application` and is shared
- Add a custom JWT validation middleware — use `JwtBearerEvents` for auth failure format
- Add soft delete (`DeletedAtUtc`) to an entity unless that entity's lifecycle specifically requires it
- Create error code classes as `static class` — must be `sealed class : ErrorCodes` for test discovery
- Pass Command or Query types directly as `[FromBody]` or `[FromQuery]` in controllers — always use a separate Request record in `Controllers/{Module}/Requests/`

---

## Solution Structure

```
MyOS/                                      ← repo root
├── CLAUDE.md
├── MyOS.slnx
├── Directory.Build.props                  ← global: net10.0, nullable, implicit usings
├── docker-compose.yml
├── docker/                                ← SQL Server init scripts
├── .env / .env.example
├── init.sql / init-db.sh
│
├── MyOS.API/                              ← Web API entry point
├── MyOS.Migrator/                         ← Database migrations + SQL view sync
│
├── MyOS.Core.Domain/                      ← Shared base entities, Language enum
├── MyOS.Core.Application/                 ← Shared CQRS contracts, Result, pagination, ICurrentUser, IUnitOfWork, IErrorTranslator, SQLKata extensions
├── MyOS.Core.Infrastructure/              ← EF Core setup, Serilog, DI extensions, CurrentUserService, UnitOfWork, ErrorTranslator
│
├── MyOS.Tests/                            ← Unit tests (translation completeness, error code conventions)
│
├── MyOS.Identity.{Layer}/                 ← legacy module (Domain / Application / Infrastructure)
│
├── MyOS.Modules.{ModuleName}.{Layer}/     ← one Domain / Application / Infrastructure triplet per business module (e.g. Notes)
│
└── src/web/                               ← Next.js frontend
```

> **Naming rule:** New modules follow `MyOS.Modules.{ModuleName}.{Layer}` (e.g. `MyOS.Modules.Notes.*`). The Identity module predates this convention and uses `MyOS.Identity.*` (without `Modules`) — do not rename it, but all new modules must use the `Modules.` form.
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

**Reference implementation:** `MyOS.Identity.Infrastructure/Extensions/DependencyInjection.cs` — registers EF configs, `JwtSettings`, repos, services, auth, and calls `AddIdentityApplication()`.

**`AddIdentityApplication()`** — internal DI extension in each module's Application project: registers MediatR + validators for this module.

**MediatR is registered per-module** (not globally). `AddCoreApplication()` adds `LoggingBehavior` and `ValidationBehavior` (in that order — logging wraps validation).

**`AddCore()` registers** (`Core.Infrastructure/Extensions/DependencyInjection.cs`): `LoggingBehavior` + `ValidationBehavior` pipeline (via `AddCoreApplication()`), `AppDbContext` (SQL Server), `QueryFactory` (SQLKata, scoped), `IUnitOfWork`, `ICurrentUser` (`CurrentUserService`).

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

**SQLKata helpers** (`Core.Application/SqlKata/QueryExtensions.cs`):
- `query.GetPagingListAsync<T>(PagingRequest, CancellationToken)` — executes COUNT + paged SELECT, returns `Result<PagingList<T>>`
- `PagingRequest.OrderBy` is validated against `T`'s public property names (case-insensitive only in the first character — e.g. `createdAtUtc` matches `CreatedAtUtc`, but `createdatutc` does not) before being converted to snake_case and passed to SQLKata. An unmatched column returns `Result.Failure(PagingErrors.InvalidOrderBy)` (→ 400) instead of letting SQL Server reject an invalid column name (→ 500)
- Inject `QueryFactory db`, build query with `db.Query(...)`, chain conditions, call extension at the end — query handlers can `return await ... .GetPagingListAsync<TDto>(...)` directly since it already returns `Result<PagingList<TDto>>`

**MediatR** is the mediator. Registered per-module via `AddIdentityApplication()` pattern.

---

## Command File Convention (Slice)

**Order within the file:** Command → Validator → Handler (matches the flow of a request).

### Legacy modules (`MyOS.Identity.*`) — per-command subfolders

Each command/query gets its own subfolder with one file.

```
Commands/
├── Register/
│   └── RegisterCommand.cs        ← RegisterCommand + RegisterCommandValidator + RegisterCommandHandler
├── Login/
│   ├── LoginCommand.cs           ← LoginCommand + LoginCommandValidator + LoginCommandHandler
│   └── (reuses Shared/AuthTokens.cs)
└── Shared/
    └── AuthTokens.cs             ← shared response DTO
```

### New modules (`MyOS.Modules.*`) — slice-per-entity

The entity-slice pattern is the single organizing principle for all three layers of a module. One folder per entity — never one folder per command or query.

| Layer | Folder structure |
|---|---|
| Application | `Notes/TextNotes/`, `Notes/CheckList/` — commands, queries, handlers, `Shared/` DTOs, optional `BusinesRules/` — `{Condition}Rule` classes implementing `IBusinessRule<T>` |
| Domain | `Notes/TextNotes/`, `Notes/CheckList/` — entities, value objects, repository interfaces |
| Infrastructure | `EntityConfigurations/TextNotes/`, `EntityConfigurations/CheckList/` — EF configs |

```
{Module}.Application/Notes/
├── TextNotes/
│   ├── CreateTextNoteCommand.cs   ← command + validator + handler
│   ├── UpdateTextNoteCommand.cs
│   ├── DeleteTextNoteCommand.cs
│   ├── GetTextNoteQuery.cs        ← query + handler
│   ├── GetTextNotesQuery.cs
│   └── Shared/
│       └── TextNoteDto.cs
└── CheckList/
    ├── CreateCheckListCommand.cs
    ├── AddCheckListItemCommand.cs
    ├── GetCheckListQuery.cs
    ├── GetCheckListsQuery.cs
    ├── ...
    └── Shared/
        ├── CheckListDto.cs
        ├── CheckListItemDto.cs
        └── CheckListSummaryDto.cs

{Module}.Domain/Notes/
├── TextNotes/
│   ├── TextNote.cs
│   └── ITextNoteRepository.cs
└── CheckList/
    ├── CheckList.cs
    ├── CheckListItem.cs
    └── ICheckListRepository.cs
```

**Response DTO naming:**
- Command results: descriptive noun describing the data, not the action — e.g., `AuthTokens` (not `LoginResponse`)
- Query results: `{Entity}Dto` suffix — e.g., `TextNoteDto`, `CheckListDto`
- Shared DTOs within a slice go in `Shared/` subfolder of that slice's folder

---

## Error Codes — Static Error Classes

Never hardcode error codes as inline strings in handlers. Use static error classes.

**Pattern:**
```csharp
// MyOS.Identity.Application/Errors/UserErrors.cs
public sealed class UserErrors : ErrorCodes
{
    private UserErrors() { } // reflection only — see ErrorCodes base class

    public static readonly Error EmailAlreadyInUse = Error.Conflict("UserErrors.EmailAlreadyInUse");
    public static readonly Error InvalidCredentials = Error.Unauthorized("UserErrors.InvalidCredentials");
    // ...
}
```

**Rules:**
- Class must be `sealed class` inheriting `ErrorCodes` (not `static class`) — enables reflection-based test discovery
- Private constructor — prevents instantiation; comment explains it's for reflection
- **No `message` in the code** — messages live in `.resx` resource files, never inline
- **Code format: `{ClassName}.{FieldName}`** — e.g. class `UserErrors`, field `InvalidCredentials` → code `"UserErrors.InvalidCredentials"`. This is enforced by a unit test.
- Use optional `parameters` dict when error message has placeholders: `Error.NotFound("UserErrors.NotFound", new Dictionary<string,string> { {"id", userId.ToString()} })`

**Location:** `{Module}.Application/Errors/{Entity}Errors.cs`
— in **Application** layer (not Domain), because `Error` type is from `Core.Application`.

**Exception — cross-cutting codes in Core:** error codes that don't belong to any business module (e.g. `PagingErrors`) live in `Core.Application/Errors/` and follow the exact same pattern: `.resx` files in `Core.Application/Resources/`, a `CoreErrorMessageProvider`, registered via `AddSingleton<IErrorMessageProvider, CoreErrorMessageProvider>()` in `AddCoreApplication()` (not per-module).

**In handlers:**
```csharp
return Result<Guid>.Failure(UserErrors.EmailAlreadyInUse);   // not: Error.Conflict("UserErrors.EmailAlreadyInUse")
```

**Messages are translated at the API boundary** — handlers never deal with message strings.

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
    Language Language { get; }
}
```

- Any module's handler can inject `ICurrentUser` to get caller context (ownership checks, audit, data scoping).
- `Id` and `Email` throw `InvalidOperationException` if accessed when `IsAuthenticated == false` — accessing them on an `[Authorize]` endpoint is always safe.
- Check `IsAuthenticated` first on endpoints that allow anonymous access.
- `Language` defaults to `Language.English` for unauthenticated requests — safe to read without checking `IsAuthenticated`.

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

`Error(Code, Message, Type, Parameters?)` — factories: `Validation(code, message, params?)` and `NotFound / Conflict / Unauthorized / Forbidden / Failure / Unexpected(code, params?, message="")`. Message defaults to `""` — resolved from `.resx` at the API boundary.

`Parameters` enables placeholder substitution in translated messages: `{email}`, `{max}`, etc.

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
    // IErrorTranslator and ICurrentUser resolved lazily from HttpContext.RequestServices
    protected IActionResult HandleResult<T>(Result<T> result)
        => result.ToActionResult(HttpContext, ErrorTranslator, CurrentUser);
}
```

`ErrorTranslator` resolves the `Error.Message` from `.resx` using `ICurrentUser.Language` before building `ProblemDetails`.

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

**Request records** — controllers never accept Command/Query types directly as `[FromBody]` or `[FromQuery]`. Always create a `sealed record` in `Controllers/{Module}/Requests/` containing only the fields the client provides (no route IDs, no `UserId` from `ICurrentUser`). The controller maps Request → Command/Query before sending.

```
Controllers/Notes/Requests/CreateTextNoteRequest.cs   ← sealed record(string Title, string Text)
Controllers/Notes/Requests/GetTextNotesRequest.cs     ← sealed record : PagingRequest (+ future filter props)
```

For `[FromQuery]` list endpoints: inherit from `PagingRequest` (which is a `record`). Future filters are added as `init` properties on the Request record — no changes needed in the Query or handler. The Request is passed directly as `PagingRequest` via polymorphism.

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

Logs with structured logging (message template format — see Logging section).

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

## Business Rules — Complex Cross-Entity Checks

Handlers follow a strict 4-step flow:

```
1. FluentValidation (pipeline) — syntax/format validation, no I/O
2. Business Rules               — everything that needs a repo/DB lookup to decide
3. Mutation                     — entity method calls (Create/Update/Revoke/...)
4. Save                         — IUnitOfWork.SaveChangesAsync (once)
```

**Rule:** business rules are pure checks — pass or fail. They never fetch-and-return data to
the handler. If the handler needs an entity for the mutation step, the handler fetches it
itself (via the repository it already has injected) BEFORE running the checks, then passes the
(possibly `null`) entity into the rule — the rule only validates its state, it does not load
it. The only exception is a rule whose check is INHERENTLY a query the handler has no other
reason to run (e.g. uniqueness by an alternate key) — it queries internally but still returns
pass/fail only, never an entity.

The handler calls ALL rules through a single `BusinessRuleChecker.CheckAsync(...)` and checks
`IsFailure` **exactly once**, regardless of how many rules there are.

**`IBusinessRule`** (`Core.Application/Abstractions/BusinessRules/`):
```csharp
public interface IBusinessRule
{
    Error Error { get; }

    Task<bool> CheckAsync(CancellationToken cancellationToken);
}
```

- **`CheckAsync`** returns `true` if the rule is satisfied, `false` otherwise — it never builds
  a `Result` itself.
- **`Error`** is the error to return when `CheckAsync` returns `false` — normally a direct
  reference to a static `{Entity}Errors.*` field. For dynamic message placeholders, return
  `SomeErrors.Code with { Parameters = ... }` — `Error` is a `record`, so `with` overrides only
  `Parameters` while reusing the existing error code/type.

**`BusinessRuleChecker.CheckAsync(ct, params IBusinessRule[] rules)`** — runs the rules
sequentially; on the first rule whose `CheckAsync` returns `false`, returns
`Result<Unit>.Failure(rule.Error)`. If all rules pass, returns `Result<Unit>.Success(Unit.Value)`.

**Pattern** (handler fetches, rules validate):
```csharp
var user = await userRepository.GetByIdAsync(currentUser.Id, cancellationToken);
var existingToken = await userRepository.GetRefreshTokenAsync(command.RefreshToken, cancellationToken);

var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
    new UserMustBeActiveRule(user),
    new RefreshTokenMustBeActiveRule(existingToken, user?.Id));

if (check.IsFailure)
    return Result<AuthTokens>.Failure(check.Error);

user!.ChangeLanguage(command.Language);
existingToken!.Revoke(replacedByToken: tokens.RefreshToken);
```

**Location:** rules ALWAYS live in a `BusinesRules/` subfolder — never directly alongside the
command/handler files.
- Legacy modules (`Identity.*`): a rule specific to one command goes in that command's
  `BusinesRules/` subfolder (`Commands/Register/BusinesRules/EmailMustBeUniqueRule.cs`); a rule
  shared across commands goes in `Commands/Shared/BusinesRules/`.
- New modules (slice-per-entity): `{Entity}/BusinesRules/{Condition}Rule.cs`.

**Naming:** `{Condition}Rule`, e.g. `EmailMustBeUniqueRule`, `UserMustBeActiveRule`,
`RefreshTokenMustBeActiveRule`.

**Implementation:** `internal sealed class`. Rules that validate an already-loaded entity take
it (nullable) as a constructor parameter and perform no I/O. Rules whose check is inherently a
query (uniqueness, existence by an alternate key) inject the repository instead. Not registered
in DI — the handler creates it via `new`. Returns `Error` values from existing static
`{Entity}Errors` classes.

**What is NOT a Business Rule:** fetching entities — always the handler's job, never the rule's;
comparisons over data already loaded by the handler; entity computed properties — those stay
inline / on the entity.

---

## Domain Layer

**Base entity** (`Core.Domain/Entities/Entity.cs`): `abstract class Entity` with `Guid Id { get; protected set; }`.

**Entity rules:**
- Constructors are `internal` — use static factory methods (`Create(...)`)
- Properties have `private set`
- Mutating methods are `internal` — add `<InternalsVisibleTo Include="MyOS.Modules.{Name}.Application" />` to Domain `.csproj` so Application handlers can call them cross-assembly without making them `public`
- Private parameterless constructor for EF Core (initialize non-nullable strings with `null!`)
- Timestamps: `CreatedAtUtc` (DateTime), `UpdatedAtUtc` (DateTime?) — add per entity as needed
- Soft delete (`DeletedAtUtc`) is a global convention but must be added explicitly per entity — do not add it unless that entity's lifecycle requires it

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

**Always map all columns explicitly** in entity configurations — do not rely on EF Core naming conventions. See `Identity.Infrastructure/EntityConfigurations/` for reference.

**Query result DTOs mapped by Dapper must be property-based records** (not positional records). Positional records lack a parameterless constructor — Dapper cannot instantiate them. Use `init` properties instead:
```csharp
// Correct — Dapper can map this
public sealed record TextNoteDto { public Guid Id { get; init; } public string Title { get; init; } = string.Empty; ... }

// Wrong — no parameterless constructor, Dapper throws
public sealed record TextNoteDto(Guid Id, string Title, ...);
```
`CheckListDto` (assembled manually in the handler, not by Dapper) may remain positional.

**Repository `GetByIdAsync` must filter soft-deleted entities** — always include `&& e.DeletedAtUtc == null` in the predicate. Without it, mutating command handlers will operate on logically deleted records (update/delete after soft-delete silently resurrects the entity).

**Infrastructure contains:** EF Core, SQLKata, repositories, external services, background services, logging config, Serilog.

**Infrastructure must NOT contain** business logic.

---

## Database

- **SQL Server 2022** (Docker: `mcr.microsoft.com/mssql/server:2022-latest`)
- Each module has its own schema:

| Module | Schema |
|---|---|
| Identity | `identity` |
| Notes | `notes` |
| Learning | `learning` |
| Finance | `finance` |
| Fitness | `fitness` |
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
│   ├── Identity/
│   └── Notes/
└── Views/
    └── Notes/
        ├── v_text_notes.sql
        ├── v_check_lists.sql
        └── v_check_list_items.sql
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
- One legacy migration (`Migrations/system/20260426_CreateSqlFileHistoryTable.cs`) uses `[Migration(2026042601)]` (date-only, no time component) — do not copy this format; all new migrations use full `YYYYMMDDHHNN`

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

**View pattern:** `CREATE OR ALTER VIEW [{schema}].[v_{name}]`

```sql
-- Views/Notes/v_text_notes.sql
CREATE OR ALTER VIEW [notes].[v_text_notes]
AS
SELECT id, user_id, title, text, created_at_utc, updated_at_utc
FROM [notes].[text_notes]
WHERE deleted_at_utc IS NULL;
```

**Rules:**
- Views expose only required columns
- Queries use views via SQLKata, not raw EF Core
- Child entity views may JOIN the parent to expose `user_id` for ownership filtering in query handlers — e.g. `v_check_list_items` JOINs `check_lists` so that queries can filter `.Where("user_id", currentUser.Id)`

---

## Internationalisation (i18n)

**Implemented.** Language is stored in DB and included as a `"language"` claim in the JWT access token.

### Language enum

`MyOS.Core.Domain/Enums/Language.cs` — shared across all modules:
```csharp
public enum Language { English = 0, Polish = 1 }
```

### Error message translation pipeline

1. Domain errors are created with **only a code** — no inline message:
   ```csharp
   Error.Unauthorized("UserErrors.InvalidCredentials")
   ```
2. `IErrorTranslator` (`Core.Application`) resolves the message from `.resx` at the API boundary using the current user's language.
3. Validation errors (from FluentValidation) carry their message directly in `Error.Message` — translator passes them through unchanged.
4. `LanguageCultureMiddleware` (`Core.Infrastructure`) sets `CultureInfo.CurrentCulture` / `CurrentUICulture` from the JWT claim so FluentValidation uses the correct language automatically.

### Resource files — per module

Each module's Application project contains `.resx` files in `Resources/`:
```
{Module}.Application/Resources/
├── {Module}Errors_en.resx
└── {Module}Errors_pl.resx
```

**Naming conventions:**
- File: `{ModuleName}Errors_{langCode}.resx` — e.g. `IdentityErrors_en.resx`
- Key format: `{ClassName}.{FieldName}` — e.g. `UserErrors.InvalidCredentials`
- Lang code derived from `Language` enum via explicit ISO code map (`English → "en"`, `Polish → "pl"`)

**Embed in csproj** (without `<LogicalName>` — let SDK compute the manifest name):
```xml
<EmbeddedResource Update="Resources\IdentityErrors_en.resx" />
<EmbeddedResource Update="Resources\IdentityErrors_pl.resx" />
```

**Placeholder substitution:** use `{key}` in translated strings, pass values via `Error.Parameters`:
```csharp
Error.NotFound("UserErrors.NotFound", new Dictionary<string,string> { {"id", id.ToString()} })
// .resx: "User with id {id} was not found."
```

### IErrorMessageProvider

Each module registers its own `IErrorMessageProvider` (singleton) in `Add{Name}Application()` via `services.AddSingleton<IErrorMessageProvider, {Name}ErrorMessageProvider>()`.

The provider builds `ResourceManager` instances from `typeof(Provider).Namespace` + language code — no magic strings.

`IErrorTranslator` (registered in `AddCore()`) collects all `IErrorMessageProvider` instances via `IEnumerable<IErrorMessageProvider>`.

### ErrorCodes base class

`MyOS.Core.Application/Abstractions/ErrorCodes.cs` — marker for reflection-based test discovery:
```csharp
public abstract class ErrorCodes
{
    protected ErrorCodes() { } // subclasses use private constructor — reflection only
}
```

All error code classes inherit `ErrorCodes`. This enables the translation completeness test to discover all error codes automatically.

### Language change endpoint

`PATCH /api/v1/users/me/language` — updates language in DB and returns new `AuthTokens` (with updated `language` claim). Client must use the new access token for subsequent requests.

### Translation tests (`MyOS.Tests`)

Three tests enforce correctness at build/CI time:

| Test class | What it checks |
|---|---|
| `TranslationCompletenessTests` | Every error code has a translation for every `Language` enum value |
| `ErrorCodeConventionTests` | `Error.Code == "{ClassName}.{FieldName}"` for every static field |
| `ResourceKeyConventionTests` | Every `.resx` key exists as a real error code (no orphans) + correct format |

All tests use `ErrorTestFixture` which discovers assemblies and resource manifests via reflection — **no magic strings**.

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

## Observability

Serilog structured logging; `traceId` and `correlationId` are included in all `ProblemDetails` responses (including auth failures).

When adding distributed tracing, health checks (`/health/live`, `/health/ready`), or metrics, follow the existing correlation-ID and structured-logging conventions.

---

## Pagination

`PagingRequest` (a `record`) and `PagingList<T>` in `Core.Application/Abstractions/Pagination/`. Max `PageSize` = 100.

`PagingRequest` is a `record` so that controller-layer list Request records can inherit from it:
```csharp
public sealed record GetTextNotesRequest : PagingRequest;
// future: { public string? TitleFilter { get; init; } }
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

**LoggingBehavior** (`Core.Application/Behaviors/LoggingBehavior.cs`) — MediatR pipeline behavior, outermost layer (registered before `ValidationBehavior`):
- Logs every command/query with parameters (`{@Request}`) at `Information` level on start
- Logs execution time + `{ErrorCode}` at `Warning` on `Result.IsFailure`
- Logs execution time at `Information` on success
- Does NOT catch exceptions — unhandled exceptions propagate to `GlobalExceptionHandlingMiddleware`

**SensitiveDataDestructuringPolicy** (`Core.Infrastructure/Logging/SensitiveDataDestructuringPolicy.cs`) — Serilog destructuring policy, registered globally in `SerilogConfiguration`. Replaces values of properties named `Password`, `Token`, `RefreshToken`, `Secret`, `SecretKey`, `Key`, `Hash`, `PasswordHash` with `[REDACTED]` in all log events.

**Seq** — planned sink. Adding it = `Serilog.Sinks.Seq` package + `.WriteTo.Seq(url)` in `SerilogConfiguration`. No structural changes needed — current setup is already Seq-compatible.

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

All projects via `Directory.Build.props`: `net10.0`, nullable enable, implicit usings, `LangVersion latest`, `GenerateDocumentationFile true`.

---

## Key NuGet Packages

| Package | Used in |
|---|---|
| MediatR | Core.Application |
| FluentValidation + DI Extensions | Core.Application, Identity.Application |
| EF Core (SqlServer) | Core.Infrastructure |
| Serilog (+Extensions.Hosting, +Sinks.Console) | Core.Infrastructure |
| SqlKata.Execution | Core.Application (QueryFactory DI + GetPagingListAsync extension) |
| FrameworkReference: Microsoft.AspNetCore.App | Core.Infrastructure (IHttpContextAccessor) |
| Microsoft.AspNetCore.Authentication.JwtBearer | Identity.Infrastructure |
| BCrypt.Net-Next | Identity.Infrastructure |
| Asp.Versioning.Mvc | MyOS.API |
| FluentMigrator | MyOS.Migrator |

---

## Testing

| Type | Tool | Purpose |
|---|---|---|
| Unit | xUnit | Translation completeness, error code conventions (`MyOS.Tests`) |
| Integration | xUnit + Testcontainers | API endpoints with real SQL Server |
| Architecture | ArchUnitNET or NetArchTest | Layer boundary enforcement |

**`MyOS.Tests/Translation/`:**
- `TranslationCompletenessTests` — all error codes have translations for all languages
- `ErrorCodeConventionTests` — `Error.Code` matches `"{ClassName}.{FieldName}"`
- `ResourceKeyConventionTests` — no orphaned `.resx` keys, correct format

Rules:
- Tests must be deterministic, isolated, not dependent on local machine state
- Integration tests use Testcontainers (real SQL Server in Docker)
- No mocking the database in integration tests
- When adding a new error code, the translation tests in CI will catch missing `.resx` entries

---

## Adding a New Module — Checklist

1. Create projects: `MyOS.Modules.{Name}.Domain`, `MyOS.Modules.{Name}.Application`, `MyOS.Modules.{Name}.Infrastructure`
2. Add schema migration: `Migrations/{Name}/YYYYMMDDHHNN_Create{Name}Schema.cs`
3. Add table migrations per entity
4. Add SQL views in `Views/{Name}/v_{name}.sql`
5. Implement domain entities extending `Entity`, repository interfaces in Domain
6. Add `<InternalsVisibleTo Include="MyOS.Modules.{Name}.Application" />` to Domain `.csproj` — enables Application handlers to call `internal` aggregate mutating methods cross-assembly
7. Create `{Entity}Errors.cs` in `{Name}.Application/Errors/` — `sealed class : ErrorCodes`, private constructor, codes only (no messages), format `{ClassName}.{FieldName}`
8. Create `{Name}Errors_en.resx` and `{Name}Errors_pl.resx` in `{Name}.Application/Resources/` with translations for every error code. Add `<EmbeddedResource Update="...">` in `.csproj` for each.
9. Create `{Name}ErrorMessageProvider` in `{Name}.Application/Resources/` — builds `ResourceManager` from `typeof(Provider).Namespace` + language code map. Register as `services.AddSingleton<IErrorMessageProvider, {Name}ErrorMessageProvider>()` in `Add{Name}Application()`.
10. Register `{Name}ErrorMessageProvider` assembly in `ErrorTestFixture.ModuleAssemblies` and `Providers` in `MyOS.Tests` so translation tests cover it automatically.
11. Implement commands and queries using entity-slice structure — one folder per entity in Application, Domain, and Infrastructure (see Command File Convention)
12. Add EF entity configurations extending `EntityConfiguration<T>` with full explicit column mapping
13. Add `Add{Name}Application()` DI extension in Application (registers MediatR + validators + `IErrorMessageProvider` for this assembly)
14. Expose `Add{Name}Module(IServiceCollection, IConfiguration)` from Infrastructure DI — registers EF configs, repos, application DI
15. Register in `Program.cs` via `builder.Services.Add{Name}Module(builder.Configuration)`
16. Add controllers in `MyOS.API/` extending `ApiControllerBase`, with `[ApiVersion("1.0")]` and `[Authorize]` where needed
