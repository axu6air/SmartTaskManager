
Angular 20 frontend for the Smart Task Management System. The backend is a separate ASP.NET Core API (Clean Architecture, REST/JSON, dynamic RBAC). This file defines how code in this repo must be written. Follow it on every change.

## Commands

```bash
npm start            # ng serve (development)
npm run build        # production build — must pass with zero warnings
npm run lint         # angular-eslint
npm run format       # prettier --write
npm test             # unit tests
npm run e2e          # Playwright end-to-end tests
```

Before declaring a task done: `npm run lint && npm test && npm run build` must all pass.

## Tech stack

- Angular 20, standalone components, zoneless change detection
- TypeScript strict mode (no `any`, no non-null assertions unless justified in a comment)
- Angular Material 20 (M3) + CDK for UI, SCSS for styles
- NgRx SignalStore (`@ngrx/signals`) for feature state
- RxJS only where streams genuinely help (HTTP, debounced search); signals for state
- Playwright for e2e

## Folder structure

```
src/app/
  core/        # singletons: auth, http (api service, interceptors), guards, layout shell, error handler
  shared/      # reusable, stateless: ui components, directives, pipes, utils, validators
  features/    # one folder per feature, each lazy-loaded with its own <feature>.routes.ts
    dashboard/ projects/ tasks/ users/ roles/ profile/
  models/      # shared TS types mirroring backend DTOs
```

Path aliases: `@core/*`, `@shared/*`, `@features/*`, `@models/*`, `@env/*`. Never use deep relative imports (`../../../`).

Inside a feature:
```
features/tasks/
  tasks.routes.ts
  data/          # api service + SignalStore for this feature
  pages/         # routed (smart) components
  components/    # presentational components used only by this feature
  models/        # feature-only types
```

Rules:
- `shared/` never imports from `features/` or `core/` stateful services.
- Features never import from other features. Move shared things to `shared/` or `models/`.
- `core/` services are `providedIn: 'root'`; feature stores are provided at the feature route level.

## Angular conventions (mandatory)

- Standalone components only. No NgModules.
- `ChangeDetectionStrategy.OnPush` on every component.
- Angular 20 file naming: `task-list.ts`, `task-list.html`, `task-list.scss` (no `.component` suffix). Class names still descriptive: `TaskList`, `TaskApi`, `TasksStore`.
- Dependency injection with `inject()`, never constructor injection.
- Signal APIs: `input()`, `input.required()`, `output()`, `model()`, `viewChild()`, `contentChild()`. No `@Input`/`@Output` decorators.
- Built-in control flow only: `@if`, `@for` (always with `track`, usually `track item.id`), `@switch`, `@defer`. No `*ngIf` / `*ngFor` / `ngClass` where `[class.x]` works.
- Derived state via `computed()`. Use `effect()` only for side effects that touch the outside world (localStorage, DOM APIs), never to sync one signal into another.
- No method calls in templates for derived values — use `computed()` or pure pipes.
- Convert observables with `toSignal()`; manual subscriptions must use `takeUntilDestroyed()`.
- Functional guards, resolvers and interceptors only.
- Strictly typed reactive forms (`FormGroup<{...}>` / `NonNullableFormBuilder`). No template-driven forms.
- Smart/presentational split: pages talk to stores; presentational components get data via inputs and emit outputs, no service injection.
- Prefer `if/else` blocks over `switch` statements in TypeScript logic.
- Keep components small (aim < 150 lines TS). Extract when a component does more than one job.

## Backend contract

- No Swagger/OpenAPI. TS interfaces in `models/` must mirror the backend C# DTOs exactly (camelCase JSON). If an endpoint or field is unknown, ask — never invent endpoints.
- All IDs are GUIDs → `string` in TS. Use a branded type alias `type Guid = string` for readability.
- All dates are `DATETIMEOFFSET` → ISO 8601 strings **with offset**. Keep them as strings in models; parse/format only through `shared/utils/date.ts` and the `offsetDate` pipe. Never send dates via `toISOString()` without considering offset loss.
- Audit fields on entities: `createdBy`, `createdAt`, `updatedBy`, `updatedAt`.
- Paging: `PagedQuery { pageNumber, pageSize, sortBy?, sortDirection?, search? }` → `PagedResult<T> { items, pageNumber, pageSize, totalCount }`.
- Errors: RFC 7807 `ProblemDetails`; validation errors arrive in `errors: Record<string, string[]>` and must be mapped onto the matching form controls.
- All HTTP goes through `@core/http/api.service.ts`. Feature `*Api` services wrap it; components never inject `HttpClient` directly.

## HTTP interceptors (order matters)

1. `authInterceptor` — attach bearer token; skip auth endpoints.
2. `refreshTokenInterceptor` — on 401, refresh once using a single shared in-flight request; retry original; logout on failure.
3. `errorInterceptor` — map to `AppError`; 403 → "no permission" toast; 404/5xx → toast; 400 validation passed to caller.
4. `loadingInterceptor` — global pending counter signal; opt out via `SKIP_LOADING` `HttpContextToken`.

## Dynamic RBAC

Authorization is permission-based and data-driven. **Never check role names in the UI.**

- Permission keys live in `@core/auth/permissions.ts` as a const object mirroring backend keys (e.g. `Permissions.Tasks.Create`). Never use raw permission strings elsewhere.
- `PermissionService` exposes `has`, `hasAny`, `hasAll`, backed by the `permissions` signal in `AuthService`.
- Templates: `*appHasPermission` directive. Routes: `permissionGuard` with `data: { permissions: [...] }`, applied as `canMatch` so unauthorized users don't download the chunk.
- Navigation menu is config data filtered by a `computed()` signal.
- Default: hide actions the user can't perform. Disable-with-tooltip only when hiding would confuse (explain in a comment).
- When permissions change (e.g. user edits own role), refresh them in `AuthService` so the UI updates immediately.
- Frontend checks are UX only; the API is the source of truth.

## State management

- One SignalStore per feature in `features/<feature>/data/`, following the reference pattern in `ProjectsStore`: `entities`, `selectedId`, `query`, `loading`, `error`, plus `load/create/update/delete`.
- Mutations that users expect to feel instant (Kanban moves, status changes) are optimistic with rollback on error.
- List query state (page, sort, filters, view mode) is synced to URL query params so refresh/back/share work.
- Logout must reset every feature store.

## UI / UX standards

- Every data view has loading (skeleton), empty, and error states.
- Destructive actions go through `ConfirmDialogService`.
- Forms: show validation on touch/submit, map server errors, disable submit while pending, warn on unsaved changes (`canDeactivate`).
- Use design tokens (SCSS variables / CSS custom properties) — no hard-coded colors or magic spacing.
- Light/dark theme via `ThemeService`; everything must look right in both.
- Responsive down to 360px; sidebar becomes a drawer on mobile.
- Accessibility: labelled controls, visible focus, focus trapped/restored in dialogs and drawers, keyboard-operable Kanban, sufficient contrast, `prefers-reduced-motion` respected.

## Performance

- Every feature lazy-loaded; preload only routes the current user is permitted to see.
- `@defer` for below-the-fold or heavy widgets (charts, comments, activity).
- Debounce search inputs (300ms) and cancel stale requests (`switchMap`).
- Respect bundle budgets in `angular.json`; justify any new dependency.

## Testing

- Unit test: interceptors (especially refresh), guards, `PermissionService`, `*appHasPermission`, stores, date utils, validators, query-param sync.
- Component tests for forms and key presentational components.
- Mock HTTP with `HttpTestingController`; never hit a real backend in unit tests.
- e2e (Playwright): login, create project, create task, move task on Kanban, restricted user cannot reach admin pages.
- Test behavior, not implementation details.

## Principles

- SOLID, DRY, KISS. Prefer the simplest solution that is correct.
- Correctness first: handle null/empty/error paths explicitly, think through edge cases (expired token mid-request, concurrent refresh, empty pages after delete, offset-aware date comparisons).
- No dead code, no commented-out code, no `console.log` in committed code.
- Don't add libraries for things Angular/CDK already provide.

## Working agreement for Claude

- Read relevant existing files before editing; match established patterns.
- Give full file contents with paths for new files; minimal diffs for edits.
- List any npm packages to install.
- If a requirement, DTO, or endpoint is unclear, ask instead of guessing.
- After changes, run lint, tests and build, and report results honestly.
