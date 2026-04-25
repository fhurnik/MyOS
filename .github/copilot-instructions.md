# Copilot Instructions for MyOS

## Project Overview

MyOS is a modular monolith Web API built with ASP.NET Core.

The system is designed as a production-grade backend that manages personal life domains such as notes, learning, finance, and fitness.

The architecture follows pragmatic Domain-Driven Design (DDD), CQRS, and clean separation of responsibilities between modules.

Copilot must generate code that is:

* production-quality
* maintainable
* consistent with architecture
* testable
* simple (avoid unnecessary abstractions)

Prefer clarity and correctness over cleverness.

---

## Architecture

The system is a **Modular Monolith**.

Each module is an independent bounded context with its own:

* Domain
* Application
* Infrastructure
* Database schema
* Migrations
* Controllers

Modules communicate only through public contracts.

Do not create tight coupling between modules.

---

## Solution Structure

Projects are organized as follows:

MyOS.sln

src/

MyOS.Api
MyOS.Migrator

Core/

MyOS.Core.Domain
MyOS.Core.Application
MyOS.Core.Infrastructure

Modules/

MyOS.Modules.Identity.Domain
MyOS.Modules.Identity.Application
MyOS.Modules.Identity.Infrastructure

MyOS.Modules.Notes.Domain
MyOS.Modules.Notes.Application
MyOS.Modules.Notes.Infrastructure

MyOS.Modules.Learning.Domain
MyOS.Modules.Learning.Application
MyOS.Modules.Learning.Infrastructure

MyOS.Modules.Finance.Domain
MyOS.Modules.Finance.Application
MyOS.Modules.Finance.Infrastructure

MyOS.Modules.Fitness.Domain
MyOS.Modules.Fitness.Application
MyOS.Modules.Fitness.Infrastructure

tests/

MyOS.UnitTests
MyOS.IntegrationTests
MyOS.ArchitectureTests

docs/

adr

---

## Naming Conventions

Always use:

MyOS.Modules.{ModuleName}.{Layer}

Examples:

MyOS.Modules.Notes.Domain
MyOS.Modules.Notes.Application
MyOS.Modules.Notes.Infrastructure

Do not use:

Shared
Common
Helpers

Use:

Core

---

## Domain Layer Rules

The Domain layer contains business logic.

Allowed:

* Entities
* Value Objects
* Domain Events
* Business Rules
* Enums
* Domain Exceptions

Domain must:

* be independent of infrastructure
* contain business rules
* protect invariants

Domain must NOT:

* depend on EF Core
* depend on ASP.NET
* depend on SQL
* contain DTOs
* contain Controllers
* contain logging
* contain database code

---

## Pragmatic DDD

Use pragmatic Domain-Driven Design.

Use domain modeling only where it provides value.

Avoid unnecessary abstractions.

Do NOT create:

* factories without real need
* repositories without real need
* interfaces for everything
* complex patterns without business value

Prefer:

simple domain models
clear business rules
explicit invariants

---

## Application Layer Rules

The Application layer contains use cases.

Allowed:

* Commands
* Queries
* Handlers
* DTOs
* Interfaces
* Validators
* Behaviors
* Mapping

Application must:

* orchestrate use cases
* coordinate domain logic
* return Result objects

Application must NOT:

* contain business rules
* access database directly
* use DbContext in Queries

---

## CQRS Rules

Commands and Queries must be separated.

Command side:

* EF Core
* Domain logic
* Transactions

Query side:

* SQL Views
* SQLKata
* DTO projections

Rule:

Queries MUST use SQLKata.

Queries MUST NOT use:

* EF Core
* DbContext
* Domain entities

---

## Result Pattern

All predictable errors must use Result pattern.

Do not throw exceptions for normal business flow.

Use Result for:

* validation errors
* not found
* conflicts
* authorization failures
* domain rule violations

Throw exceptions only for:

* system failures
* infrastructure failures
* unexpected errors

---

## Error Types

Use standard error categories:

Validation
NotFound
Conflict
Unauthorized
Forbidden
Failure
Unexpected

---

## Global Error Handling

Global exception handling must be implemented in API.

Always return ProblemDetails response.

Include:

* status
* message
* traceId
* correlationId

Do not expose stack traces in production.

---

## API Rules

Use Controllers.

Do not use Minimal API.

Routes must follow REST conventions.

Use:

GET /api/v1/resource
GET /api/v1/resource/{id}
POST /api/v1/resource
PUT /api/v1/resource/{id}
DELETE /api/v1/resource/{id}

Always:

* use API versioning
* validate input
* return proper status codes
* use DTOs
* use cancellation tokens

Never:

* return entities directly
* access DbContext from controllers
* implement business logic in controllers

---

## Database Rules

Use SQL Server.

Each module must have its own schema.

Example:

identity.*
notes.*
learning.*
finance.*
fitness.*

All schema changes must use FluentMigrator.

Never use:

EF Core migrations.

---

## SQL Views

Use SQL Views for read models.

Views must be created in migrations.

Example:

notes.v_notes

Views must:

* be optimized
* be stable
* expose only required columns

---

## Soft Delete

Use soft delete globally.

Always use:

DeletedAtUtc

Do not physically delete records unless explicitly required.

---

## Dependency Injection

All dependencies must be registered via DI.

Use:

Scoped for business services
Transient for lightweight components
Singleton only when safe

Do not use static services.

---

## Infrastructure Rules

Infrastructure contains:

* EF Core
* SQLKata
* Repositories
* External services
* Background services
* Logging
* Configuration

Infrastructure must not contain business logic.

---

## Testing Rules

Every feature must be testable.

Use:

Unit tests for domain logic
Integration tests for API
Architecture tests for boundaries

Use Testcontainers for integration tests.

Tests must:

* be deterministic
* be isolated
* not depend on local machine state

---

## Observability

Always support:

Structured logging
Correlation ID
Health checks
Metrics

Expose endpoints:

/health/live
/health/ready

---

## Security Rules

Always:

Validate input
Use JWT authentication
Use refresh tokens
Check user ownership
Protect sensitive data

Never:

Store passwords in plain text
Trust user input

---

## Things Copilot Must Never Do

Never:

put business logic in controllers
use DbContext inside queries
create unnecessary abstractions
create static state
bypass validation
mix layers
delete records physically by default
create tight coupling between modules

---

## Code Quality

Always generate:

clean
readable
maintainable
production-ready code

Prefer:

simple solutions
explicit logic
predictable behavior

Avoid:

clever tricks
overengineering
magic behavior

---

## Final Rule

If uncertain:

choose the simplest correct solution
follow architecture rules
do not invent new patterns

---

## Module Registration Rules

Each module must expose a single public registration method.

Use:

public static IServiceCollection Add{ModuleName}Module(
    this IServiceCollection services,
    IConfiguration configuration)

This method is responsible for:

- registering module dependencies
- registering DbContext
- registering repositories
- registering services
- registering background services if needed

The API project must not register internal module services directly.

---

## Transaction Rules

All command handlers must be executed within a database transaction.
Never call SaveChanges more than once per command handler.

Use:

- UnitOfWork
- Transaction behavior in MediatR pipeline

Never:

- start transactions in controllers
- start transactions in repositories
- perform multiple SaveChanges calls in a single command

---

## Idempotency Rules

Commands that may be retried must be idempotent.

Examples:

- Create operations
- Payment operations
- External integrations

Handlers must safely handle repeated execution of the same command.

---

## Logging Rules

Always log:

- errors
- warnings
- important domain events

Never log:

- passwords
- tokens
- personal sensitive data

Use structured logging.

Include:

- correlationId
- userId when available

---

## Migration Naming Rules

Each module must have its own directory for migrations.

Use folder structure:

Migrations/
  Identity/
  Notes/
  Learning/
  Finance/
  Fitness/

Migration names must be globally unique and ordered by date.

Use format:

YYYYMMDD_Action_Description

Examples:

Migrations/Identity/20260425_CreateUsersTable.cs
Migrations/Notes/20260426_CreateNotesTable.cs
Migrations/Notes/20260427_CreateNotesView.cs

Never rename existing migration files.
Never modify executed migrations.
Create a new migration instead.