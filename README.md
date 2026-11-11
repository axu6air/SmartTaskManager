# Smart Task Manager

A full-stack task management system: an ASP.NET Core 10 Web API and an Angular 21 single-page app. Teams organise work into projects, track tasks on a list or a Kanban board, and can ask an AI model to rewrite a task description into a clear, actionable one.

## Project Overview

### What it does

- **Projects:** create, edit, delete (soft delete), search, sort and page through projects; manage each project's members and managers.
- **Tasks:** create, edit, delete, assign, set priority (Low, Medium, High, Critical) and due date, and move tasks through To do → In progress → Completed / Cancelled. Lists support keyword search, filtering by status, priority and assignee, sorting and server-side pagination.
- **Task board:** a Kanban view with drag-and-drop status changes.
- **Dashboard:** totals, tasks by status, project progress and upcoming deadlines.
- **AI assistant:** improves a task or project description (grammar, clarity, a professional tone, more detail for short text, an actionable checklist). The suggestion is previewed and only saved when the user applies it.
- **Accounts:** sign-in with JWT access tokens and rotating refresh tokens; users with the *Manage Users* permission register new accounts.

**Access control.** Authorization is permission-based, not role-name based. Roles (Admin, Project Manager, Team Member) are data: each role holds permissions such as `tasks.update_status`, each with a scope (Assigned, Managed, Member or All). For example, a Team Member may change the status of tasks *assigned to them*, while a Project Manager may change any task in projects *they manage*. Admins edit these grants at runtime on the Roles & permissions page. The API enforces every check; the UI only hides what a user can't do.

**Design principles.** Clean Architecture with CQRS (MediatR), a repository layer over EF Core, `Result<T>` instead of exceptions for expected failures, one consistent response envelope, and validation at every entry point.

## Technology Stack

| Area | Technologies |
| --- | --- |
| Backend | .NET 10, ASP.NET Core Minimal APIs, MediatR 12, FluentValidation |
| Data | SQL Server, EF Core 10 (database-first: the SQL script is the source of truth) |
| Security | JWT bearer tokens, refresh-token rotation with reuse detection, ASP.NET Core Identity password hashing, permission-based authorization, rate limiting, CORS, HTTPS redirection |
| Observability | Serilog (console), health checks (`/health/live`, `/health/ready`) |
| AI | OpenRouter (OpenAI-compatible API) through Microsoft.Extensions.AI |
| Backend tests | xUnit, FluentAssertions, NSubstitute |
| Frontend | Angular 21 (standalone components, signals, zoneless change detection), TypeScript (strict), Angular CDK (dialogs, menus, drag-and-drop, focus trapping), NgRx SignalStore, RxJS |
| Styling | SCSS with design tokens; light and dark themes; responsive down to 360 px |
| Frontend tests | Vitest via the Angular test builder, angular-eslint |

## Setup Instructions

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+, 22.12+ or 24 (required by Angular 21), with npm
- SQL Server 2019 or later (LocalDB, Express, Developer or a Docker container)
- An [OpenRouter](https://openrouter.ai/) API key for the AI feature (a free key works)

### 1. Configure the API

Secrets are never committed. `appsettings.json` contains placeholders; real values go in .NET user secrets:

```bash
cd server/src/SmartTaskManager.Api

dotnet user-secrets set "Database:ConnectionString" "Server=localhost;Database=SmartTaskManager;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:SecretKey" "<a random string of at least 32 characters>"
dotnet user-secrets set "OpenRouter:ApiKey" "<your OpenRouter API key>"
```

All three are validated at startup; the API refuses to start if one is missing. Other settings (token lifetimes, CORS origins, rate limits, the AI model) are in `appsettings.json`.

### 2. Run the API

```bash
cd server
dotnet run --project src/SmartTaskManager.Api --launch-profile https
```

The API listens on `https://localhost:7290` (and `http://localhost:5235`). Check it with `https://localhost:7290/health/ready`.

### 3. Run the web app

```bash
cd client
npm install --legacy-peer-deps
npm start
```

Open `http://localhost:4200`. The dev server proxies `/api` to `https://localhost:7290` (see `client/src/proxy.conf.json`), so no CORS setup is needed locally.

> `--legacy-peer-deps` works around an npm 11 resolver crash on this dependency tree; the installed tree has no peer conflicts.

### Demo accounts (Development only)

All use the password `password123`. The sign-in page shows one-click buttons for them in development builds.

| Email | Role | Can, for example |
| --- | --- | --- |
| `admin@email.com` | Admin | everything, including roles and registering users |
| `manager1@email.com` | Project Manager | manage their own projects' members and tasks |
| `member11@email.com` | Team Member | view their projects, move tasks assigned to them |

### Run the tests

```bash
# Backend
cd server
dotnet test SmartTaskManager.slnx

# Frontend (on Windows run this from the D:\ path, with an uppercase drive letter:
# from d:\ Vitest fails every file with "failed to find the runner")
cd client
npm run lint
npm test -- --watch=false
npm run build
```

## API Overview

**Base URL:** `/api`. Every request except sign-in, refresh and logout needs `Authorization: Bearer <access token>`.

**Response envelope.** Every response uses the same shape:

```json
{ "success": true, "data": { }, "message": null, "errors": null, "traceId": "0HN..." }
```

Validation failures return `400` with field messages in `errors` (for example `{ "Email": ["Enter a valid email address, like name@company.com."] }`). Other errors use `401`, `403`, `404`, `409`, `429` (rate limited) and `503` (AI unavailable), each with a readable `message`.

**Paging.** List endpoints take `page` and `pageSize` (1–100) and return `{ items, page, pageSize, totalCount }`.

### Auth: `/api/auth` (stricter rate limit)

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| POST | `/login` | anonymous | Email + password → access token, refresh token, expiry |
| POST | `/refresh` | refresh token | New token pair; the old refresh token is revoked (reuse revokes the whole session family) |
| POST | `/logout` | refresh token | Revokes the refresh token |
| GET | `/me` | signed in | Profile, role names and a map of permission → scope |
| POST | `/register` | `users.manage` | Create an account (`fullName`, `email`, `password`, optional `roleId`; default role otherwise) |

### Projects: `/api/projects`

| Method | Route | Permission | Purpose |
| --- | --- | --- | --- |
| GET | `/` | `projects.view` | List visible projects. Query: `search`, `sortBy` (`name`, `startDate`, `endDate`, `createdAt`), `sortDirection` (`asc`/`desc`) |
| GET | `/{id}` | `projects.view` | Project with member IDs |
| POST | `/` | `projects.create` | Create; the creator becomes the first manager |
| PUT | `/{id}` | `projects.update` | Update name, description, dates |
| DELETE | `/{id}` | `projects.delete` | Soft delete |
| GET | `/{id}/members` | `projects.view` | Members with names and manager flag |
| PATCH | `/{id}/assign` | `projects.manage_members` | Add a member or change their manager flag |
| DELETE | `/{id}/members/{userId}` | `projects.manage_members` | Remove a member and unassign their tasks (the last manager can't be removed) |

### Tasks: `/api/projects/{projectId}/tasks`

| Method | Route | Permission | Purpose |
| --- | --- | --- | --- |
| GET | `/` | `tasks.view` | List tasks. Query: `search`, `status`, `priority`, `assigneeId`, `sortBy` (`title`, `dueDate`, `priority`, `status`, `createdAt`), `sortDirection` |
| GET | `/{id}` | `tasks.view` | One task |
| POST | `/` | `tasks.create` | Create (due date can't be in the past) |
| PUT | `/{id}` | `tasks.update` | Update title, description, priority, due date |
| PATCH | `/{id}/status` | `tasks.update_status` | Change status |
| PATCH | `/{id}/assign` | `tasks.assign` | Assign to a project member, or unassign |
| DELETE | `/{id}` | `tasks.delete` | Soft delete |

### Roles, users and AI

| Method | Route | Permission | Purpose |
| --- | --- | --- | --- |
| GET | `/api/roles` | `roles.view` | Permission catalogue and every role's grants |
| PUT | `/api/roles/{roleId}/permissions/{permissionId}` | `roles.manage` | Grant a permission with a scope (system roles are locked) |
| DELETE | `/api/roles/{roleId}/permissions/{permissionId}` | `roles.manage` | Revoke a permission |
| GET | `/api/users?search=` | `users.view` | Find up to 20 active users by name or email |
| POST | `/api/ai/task-description` | `ai.enhance_description` | Suggest an improved title and description; nothing is saved |

**Health:** `GET /health/live` (process is up) and `GET /health/ready` (database reachable), anonymous and not rate limited.

A Postman collection is in [`postman/`](postman/SmartTaskManager.postman_collection.json), and `server/src/SmartTaskManager.Api/SmartTaskManager.Api.http` has ready-made requests. The AI prompt design is documented in [`server/PROMPTS.md`](server/PROMPTS.md).

## Folder Structure

```
SmartTaskManager/
├── database/
│   └── rbac-schema.sql              Full schema and reference data (roles, permissions, grants)
├── postman/                         API collection
├── server/
│   ├── SmartTaskManager.slnx
│   ├── PROMPTS.md                   AI prompt design
│   ├── src/
│   │   ├── SmartTaskManager.Domain/          Entities and business rules; no dependencies
│   │   ├── SmartTaskManager.Application/     Use cases (commands, queries, handlers, validators),
│   │   │   ├── Common/                       abstractions, security scopes, paging, sorting
│   │   │   └── Features/                     Auth, Projects, Tasks, Roles, Users, Ai
│   │   ├── SmartTaskManager.Infrastructure/  EF Core, repositories, JWT and password services,
│   │   │                                     permission service, OpenRouter client, DB initialisation
│   │   └── SmartTaskManager.Api/             Minimal API endpoints, authorization, CORS,
│   │                                         rate limiting, logging, health checks, exception handling
│   └── tests/
│       ├── SmartTaskManager.Domain.UnitTests/
│       ├── SmartTaskManager.Application.UnitTests/
│       ├── SmartTaskManager.Infrastructure.UnitTests/
│       └── SmartTaskManager.Api.IntegrationTests/
└── client/                          Angular app
    └── src/app/
        ├── core/                    Singletons: auth (session, permissions, guards), HTTP
        │                            (API service, interceptors), layout shell, toasts, theme,
        │                            shared workspace data (projects, members, tasks)
        ├── shared/                  Stateless building blocks: UI components, pipes,
        │                            validators, date and query-param utilities
        ├── features/                Lazy-loaded pages: auth (sign-in, register), projects,
        │                            board, dashboard, roles
        └── models/                  TypeScript types mirroring the API's DTOs
```

Dependencies point inward on the server (Api → Infrastructure → Application → Domain). On the client, features never import from each other; anything shared lives in `core/`, `shared/` or `models/`.
