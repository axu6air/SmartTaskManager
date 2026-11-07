# CLAUDE.md

Guidance for Claude when working in this repository.

## Project

**Smart Task Management System**: the backend for a job-interview take-home assignment (.NET Full Stack role). It is an ASP.NET Core Web API covering projects, tasks, a dashboard, JWT auth with refresh tokens, dynamic permission-based authorization, and an AI endpoint that improves task descriptions.

Reviewers grade **architecture, code quality, security, design patterns, readability and maintainability** above feature count. Prefer a smaller, correct, well-structured change over a large one.

Backend only. Do not add frontend code to this repository.

## Guiding principles: SOLID, DRY, KISS

Apply these to every change. When they pull in different directions, **KISS wins**: the simplest design that is correct and readable.

- **Single Responsibility:** one reason to change per class. A handler handles one use case. An endpoint maps HTTP to a command or query and back, nothing more.
- **Open/Closed:** add behavior by adding new classes (a new handler, endpoint file, validator, pipeline behavior), not by growing `if` chains in shared code.
- **Liskov Substitution:** implementations of an abstraction must honour its contract. A fake `ITaskDescriptionEnhancer` behaves like the real one, including its failure results.
- **Interface Segregation:** small, focused interfaces (`ICurrentUser`, `ITokenService`, `IPermissionService`). No "god" service interfaces.
- **Dependency Inversion:** Application defines abstractions; Infrastructure implements them. High-level code never references concrete infrastructure.
- **DRY:** one source of truth for each piece of knowledge: permission codes, error definitions, paging rules, the response envelope, the `Result` → HTTP mapping. Extract only real, repeated knowledge. Two pieces of code that look alike but change for different reasons are **not** duplication.
- **KISS:**
  - No speculative abstractions, no generic repository over EF Core, no base classes that exist "in case".
  - No new package when a few lines of clear code will do.
  - Prefer plain, readable code to clever code.

## Tech stack

- .NET 10, ASP.NET Core **Minimal APIs**. No MVC controllers.
- EF Core 10, **database-first**, with SQL Server. No EF migrations.
- MediatR 12.x (Apache-2.0; do not upgrade to 13+, which needs a license key)
- FluentValidation, Serilog, ASP.NET Core rate limiting, health checks
- OpenAPI: the built-in `Microsoft.AspNetCore.OpenApi` document only. **No Swagger (Swashbuckle)** and no API versioning.
- Tests: xUnit, FluentAssertions, NSubstitute, `Microsoft.AspNetCore.Mvc.Testing`, **EF Core SQLite** (in-memory), NetArchTest
- Package versions are managed centrally in `Directory.Packages.props`. Never put a `Version` on a `PackageReference` in a `.csproj`.

## Solution structure

```
SmartTaskManager.slnx
src/
  SmartTaskManager.Domain/            Entities, enums, value objects, domain exceptions. No dependencies.
  SmartTaskManager.Application/       CQRS commands/queries, handlers, validators, DTOs, abstractions, pipeline behaviors.
  SmartTaskManager.Infrastructure/    EF Core DbContext + hand-written configurations, interceptors, startup seeding,
                                      JWT/refresh tokens, password hashing, permission service, AI client.
  SmartTaskManager.Api/               Minimal API endpoints, auth/authorization setup, middleware, exception handler, Program.cs.
tests/
  SmartTaskManager.Domain.UnitTests/          Entity invariants and domain rules.
  SmartTaskManager.Application.UnitTests/     Handlers and validators (dependencies mocked).
  SmartTaskManager.Infrastructure.UnitTests/  Interceptors, token services, permission/scope evaluation, AI response parsing.
  SmartTaskManager.Api.IntegrationTests/      Real HTTP via WebApplicationFactory + SQLite in-memory; also the architecture tests.
database/
  rbac-schema.sql                     Full current schema + reference seed data (source of truth).
  changes/                            Ordered change scripts: 001_<description>.sql, 002_..., applied in order.
```

## Commands

```bash
dotnet build SmartTaskManager.slnx
dotnet test SmartTaskManager.slnx            # no Docker needed; tests use SQLite in-memory
dotnet run --project src/SmartTaskManager.Api
```

Before saying a task is finished: `dotnet build` and `dotnet test` must pass, with **zero warnings** (`TreatWarningsAsErrors` is on).

## Architecture rules (enforced by architecture tests, never break them)

- **Domain** references nothing.
- **Application** references Domain only. No ASP.NET Core, Serilog or HTTP types, except `Microsoft.EntityFrameworkCore` for `IApplicationDbContext`.
- **Infrastructure** references Application.
- **Api** references Application, and Infrastructure only for DI registration in `Program.cs`.
- Each layer exposes one `DependencyInjection.cs` (`AddApplication`, `AddInfrastructure`). `Program.cs` stays short and only calls extension methods.

### Feature organization

Application code is organized by feature, never by technical type:

```
Application/Features/<Feature>/Commands/<UseCase>/  <UseCase>Command.cs, <UseCase>CommandHandler.cs, <UseCase>CommandValidator.cs
Application/Features/<Feature>/Queries/<UseCase>/   <UseCase>Query.cs, <UseCase>QueryHandler.cs, <UseCase>Response.cs
Api/Endpoints/<Feature>Endpoints.cs                 one static class per feature with a Map<Feature>Endpoints extension method
```

When adding a use case, also add its validator, a unit test, and, for a new endpoint, an integration test covering 401, 403 and the success path.

## Coding conventions

- **Prefer `if`/`else` over `switch` expressions.**
- File-scoped namespaces. The namespace matches the folder path.
- `_camelCase` private fields, `Async` suffix on async methods, `var` when the type is apparent.
- Async all the way: no `.Result`, no `.Wait()`, no `async void`. Pass `CancellationToken` through every handler and every EF call.
- Constructor injection only. Primary constructors are fine.
- No magic strings: permission codes, policy names, rate-limit policy names and config section names live in constants or options classes.
- Options classes are validated at startup (`ValidateDataAnnotations().ValidateOnStart()`).
- No commented-out code. No TODO without an explanation.
- Pay close attention to logical correctness. Think through edge cases (nulls, empty collections, boundaries, concurrency) before writing code.

## Domain rules

- The task entity is `ProjectTask`, never `Task` (clashes with `System.Threading.Tasks.Task`).
- Entities have private setters, a private parameterless constructor for EF, static factory methods, and behavior methods that enforce invariants. Never set state directly from handlers.
- Enum values match the schema and start at 1:
  - `ProjectTaskStatus`: ToDo = 1, InProgress = 2, Completed = 3, Cancelled = 4
  - `ProjectTaskPriority`: Low = 1, Medium = 2, High = 3, Critical = 4
- `CompletedAt` is set if and only if Status is Completed. `ProjectTask.ChangeStatus` maintains this.
- A task's assignee must be a member of that task's project. The database enforces this with a composite FK, and the domain must also check it.
- Project managers are `ProjectMembers` rows with `IsManager = true`. There is no `ManagerId` on `Project`.
  - The creator of a project is added as a manager member.
  - A project must keep at least one manager.
- Due dates cannot be in the past **on creation**. This is a validator rule, not a database constraint.

## Database (database-first) and EF Core

### Database-first workflow

The database is designed and changed in SQL. EF Core only maps to it.

- `database/rbac-schema.sql` is the **source of truth**. It always contains the full, current schema.
- **To change the schema:**
  1. Add a new numbered script in `database/changes/` (e.g. `003_add_task_labels.sql`).
  2. Apply the same change to `rbac-schema.sql`.
  3. Update the domain entity and its EF configuration by hand to match.
  4. Ask before making any schema change.
- **Never** run `dotnet ef migrations` and never call `Database.Migrate()`. There is no `Migrations` folder.
- **Domain entities are hand-written and rich**, not scaffolded. `dotnet ef dbcontext scaffold` may be run into a throwaway folder **for reference only**; never commit its output and never replace domain entities with scaffolded classes.
- **EF configurations** (`IEntityTypeConfiguration<T>`, one per entity in `Infrastructure/Persistence/Configurations/`) must mirror the schema exactly: table and column names, types, lengths, nullability, keys, FKs and delete behavior. When a mapping and the schema disagree, the schema wins.
- **Reference seed data** (roles, permission modules, permissions, role grants) lives in the SQL script. Demo users are created at app startup through the real password hasher, in Development only, and only if they don't exist.

### Mapping and query rules

- **Keys** are `Guid`:
  - Configure them with `ValueGeneratedOnAdd()`, so EF Core's default `SequentialGuidValueGenerator` creates sequential GUIDs client-side.
  - **Never** use `Guid.NewGuid()` or `Guid.CreateVersion7()` for keys. Neither is sequential in SQL Server's `uniqueidentifier` sort order, so both fragment the clustered indexes.
- **Timestamps:** every date/time is `DateTimeOffset`. Get the current time from the injected `TimeProvider` in UTC, never `DateTime.Now` or `DateTimeOffset.Now`.
- **Audit columns:** every table has `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`. The `AuditableEntityInterceptor` sets them; do not set them by hand.
- **Soft delete:** soft-deletable entities (`Project`, `ProjectTask`) use global query filters, and `SoftDeleteInterceptor` turns deletes into updates. Use `IgnoreQueryFilters()` only deliberately, with a comment explaining why.
- **Concurrency:** `RowVersion` is a concurrency token. Map `DbUpdateConcurrencyException` to a 409 Conflict.
- **Queries:**
  - Use `AsNoTracking()` and project directly to DTOs with `Select`. Never load entities just to map them.
  - Aggregate in the database (`GroupBy`, `CountAsync`), never in memory.
- **Sorting:** only through the whitelist helper (allowed field name → expression). Never build sort expressions from raw request strings.
- **Paging:** `pageSize` is clamped to 1–100.

### SQLite for tests: keep the model provider-neutral

Tests run on SQLite in-memory, while production runs on SQL Server. Keep the EF model portable, and keep the few unavoidable differences in **one place**.

- **Raw SQL in the model:** write any filter, check constraint or default SQL with plain, unbracketed column names (e.g. `IsDeleted = 0`, not `[IsDeleted] = 0`), so it is valid in both SQL Server and SQLite. Avoid SQL Server-only functions in the model.
- **One adjustment class:** all SQLite-specific differences live in a single `SqliteModelAdjustments` class. It is applied from `OnModelCreating` only when `Database.IsSqlite()` is true:
  - **`DateTimeOffset`:** convert with `DateTimeOffsetToBinaryConverter`. SQLite cannot compare or order `DateTimeOffset` values otherwise.
  - **`RowVersion`:** SQLite has no `rowversion`. Treat it as a plain concurrency token whose value the interceptor sets to new random bytes on every update.
  - **Defaults:** remove SQL Server-only default SQL such as `NEWSEQUENTIALID()` and `SYSDATETIMEOFFSET()`. The app supplies those values anyway.
  - **Nothing else:** no other provider branches anywhere in the codebase.
- **Test database setup:**
  - Create the schema with `EnsureCreated()` from the EF model. `rbac-schema.sql` is SQL Server-only and is not run against SQLite.
  - Keep one open `SqliteConnection` for each test's lifetime. An in-memory database disappears when its connection closes.
  - Foreign keys must be enforced (`PRAGMA foreign_keys = ON`; this is the default with EF Core's SQLite provider).
- **What SQLite does not prove:** SQL Server-specific behavior, such as exact types, `rowversion` semantics, filtered-index behavior and real query plans. Before submission, run `rbac-schema.sql` against a real SQL Server and smoke-test the API against it.

## Authorization (dynamic RBAC)

- **Check permissions, never roles.** Endpoints call `.RequirePermission(Permissions.Tasks.Update)`, an extension over `RequireAuthorization` backed by a permission policy provider. **Never** use role-based authorization or check role names in code. Roles are data that Admins edit at runtime.
- **Permission codes** live in `Application/Common/Security/Permissions.cs` and must match `dbo.Permissions.Code` exactly. A new permission needs a constant, a row in the SQL schema and change script, and a grant to the Admin role.
- **Permissions are resolved server-side** per request through `IPermissionService`, cached with `HybridCache` keyed by user ID. **They are not stored in the JWT.** Invalidate the cache whenever a user's roles or a role's grants change.
- **Scopes:** each grant has a `PermissionScope` flags value: Assigned = 1, Managed = 2, Member = 4, All = 8.
  - Combine a user's scopes across roles with **bitwise OR**.
  - Grant access if **any** set flag's condition holds.
  - Scopes are not a hierarchy, so never compare them with `>` or `<`.
- **Enforce scope in two places:**
  - in command handlers for single-resource actions (update, delete, status change, assign);
  - as a `Where` filter in list and dashboard queries, so users only ever receive rows they may see.
- **Guardrails:**
  - System roles (`IsSystem`) cannot be edited or deleted.
  - The last active Admin cannot be removed or deactivated.
  - Self-registration assigns only the default role.

## Security

- **Access tokens:** short-lived (from config, default 15 minutes). Validate issuer, audience, lifetime and signing key, with `ClockSkew` of 30 seconds or less.
- **Refresh tokens:**
  - Generate them as random bytes and store only their SHA-256 hash.
  - Rotate on every refresh.
  - If a revoked token is reused, revoke its whole `FamilyId`.
  - Logout revokes the token server-side.
- **Passwords:** hashed only through the `PasswordHasher<T>` adapter. Never write custom hashing.
- **Secrets:** never in committed files. `appsettings.Example.json` documents every key; real values come from user-secrets or environment variables.
- **Logging:** never log passwords, tokens, refresh tokens, authorization headers or request bodies of auth endpoints.
- **Rate limiting:** stricter policies on `auth` and `ai` endpoints, partitioned by user ID, or by IP when the request is anonymous.
- **CORS:** allows only origins from config. No wildcard origins.

## API conventions (Minimal APIs)

- **Routes** start with `/api` (no versioning). Nest tasks under projects (`/api/projects/{projectId}/tasks`).
- **Route groups:** each feature registers its endpoints in its own `Map<Feature>Endpoints` extension method, using `MapGroup` with shared tags, authorization and rate-limit policy. `Program.cs` only calls these methods.
- **Thin handlers:** an endpoint lambda or static handler method maps the request to a command or query, sends it via `ISender`, and maps the `Result` to an `IResult` with **one shared extension** (`ToHttpResult()`). No business logic in endpoints.
- **Narrow endpoints:** use dedicated endpoints for narrow actions (`PATCH .../status`, `PATCH .../assign`) so each maps to one permission.
- **Results, not exceptions:** handlers return `Result`/`Result<T>` for expected failures. Throw only for truly exceptional cases.
- **Response shape:** every response, including validation errors and 500s, uses the `ApiResponse<T>` envelope (`success`, `data`, `message`, `errors`, `traceId`).
- **Status codes:**

  | `Error.Type` | Status |
  |---|---|
  | Validation | 400 |
  | Unauthorized | 401 |
  | Forbidden | 403 |
  | NotFound | 404 |
  | Conflict | 409 |

- **Global exception handler:** never exposes stack traces outside Development.
- **Validation:** runs only in the MediatR `ValidationBehavior`, never by hand inside handlers or endpoints.
- **API documentation:** add `.WithName()`, `.WithSummary()` and `.Produces<T>()` metadata so the built-in OpenAPI document (`/openapi/v1.json`) describes every endpoint. A Postman collection can be generated from that document.

## AI feature

- **Abstraction:** `ITaskDescriptionEnhancer` lives in Application; the GitHub Models implementation lives in Infrastructure. Keep it swappable, and use a fake implementation in tests.
- **Prompt-injection defense:**
  - Wrap user text in delimiters, and tell the system prompt that delimited content is data to rewrite, never instructions.
  - Require structured JSON output and validate it before returning.
- **Robustness:** enforce input length limits, apply a timeout and retry, and on failure return a graceful error rather than a 500.
- **Suggestions only:** the endpoint returns a suggestion and never saves it.
- **Prompt documentation:** any change to the prompt must be reflected in `PROMPTS.md`.

## Testing

- **Unit tests:** no database, no network. Mock abstractions with NSubstitute.
- **Integration tests:**
  - Use `WebApplicationFactory` with the DbContext registration replaced by SQLite in-memory (see the SQLite rules above), plus a fake `ITaskDescriptionEnhancer`.
  - Never use the EF in-memory provider. It ignores constraints and relational behavior.
- **Test names:** `Method_Scenario_ExpectedResult`.
- **Bug fixes:** start with a failing test that reproduces the bug.

## Git

- Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`), one concern per commit.
- Never commit secrets, `appsettings.*.local.json`, `bin/` or `obj/`.

## When unsure

Ask before:
- changing the database schema;
- adding a new package;
- weakening a security rule;
- deviating from the architecture rules above.

When you make a judgment call, state it in your summary.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
