# Squad Decisions

## Active Decisions

### 1. Project Structure — Elevator Dispatch Demo
- **Date:** 2026-05-11T10:52:21.134-05:00
- **Author:** Mal
- **Status:** Decided

#### Co-hosted Blazor WASM (API hosts UI)
- `ElevatorApi` hosts `ElevatorUI` via `UseBlazorFrameworkFiles()`. Both share the same origin.
- Avoids CORS config complexity and dynamic port wiring in Aspire.
- AppHost only registers one project (`elevator-api`).

#### xUnit for tests
- Use xUnit per PRD specification, not MSTest.
- PRD explicitly specifies xUnit.

#### ElevatorSimulation as class library with hosting abstractions
- `ElevatorSimulation` is a `Microsoft.NET.Sdk` class library.
- `SimulationEngine` extends `BackgroundService` but library takes `Microsoft.Extensions.Hosting.Abstractions` NuGet package.
- Clean separation — simulation logic has no web dependencies.

#### Project layout
```
src/
  ElevatorSimulation/      ← domain models + BackgroundService engine
  ElevatorApi/             ← API + SignalR hub + hosts WASM
  ElevatorUI/              ← Blazor WASM (co-hosted by ElevatorApi)
  ElevatorAppHost/         ← Aspire orchestration (registers elevator-api only)
  ElevatorServiceDefaults/ ← Aspire observability defaults
  ElevatorTests/           ← xUnit suite
ElevatorDemo.sln
```

### 2. UI Shell & Visual Refresh — Blazor WASM Dashboard
- **Date:** 2026-05-11
- **Author:** Kaylee
- **Status:** Applied

#### Template Cleanup
- Removed sidebar NavMenu, Counter page, and Weather page — irrelevant to single-page simulation dashboard.
- Eliminated fixed 250 px left gutter and Bootstrap toggle button; reclaimed viewport space.

#### Unified Header Shell
- Single sticky `<header role="banner">` in MainLayout.razor with app title and "Live Simulation" tagline pill.
- Removed competing title bars; canonical app name appears only in persistent header.
- Main content area fills remaining viewport.

#### CSS Design-Token System (pp.css)
- All colours, radii, and shadows defined as CSS custom properties on `:root`.
- Component-scoped CSS files reference via `var(--token, fallback)` for resilience.

#### Elevator Colour Palette (Modern Vivid Tokens)
- EV-01: #22c55e → #16a34a (green)
- EV-02: #3b82f6 → #2563eb (blue)
- EV-03: #a855f7 → #9333ea (purple)
- EV-04: #94a3b8 → #64748b (slate)

#### Status Bar Replaces Hero Banner
- Large hero-banner replaced with compact status-bar card showing live tick/queued/waiting stats and status pill.
- Titles centralized in persistent header, not ephemeral page content.

#### Trade-offs & Future
- No dark mode wiring yet; design token foundation enables easy addition via `@media (prefers-color-scheme: dark)`.
- Not using CSS Grid for outer layout (block flow sufficient); can revisit if multi-section pages added.

### 3. Elevator Status Row Alignment Fix
- **Date:** 2026-05-11
- **Author:** Kaylee (Frontend Dev)
- **Status:** Applied

#### Problem
The bottom elevator status row was visually misaligned with shaft columns above it due to a two-level grid nesting structure. The shaft cells in `floor-row` use nested grids (outer `130px 1fr 90px`, inner `repeat(4, 1fr)`), while the old `shaft-headers` used a flat grid. The `border-right: 1px` on `.shaft-row` shrinks its content box, creating cumulative pixel drift (~1 px by column 4), causing visible misalignment at certain viewport widths.

#### Solution
Restructured status row to mirror the exact two-level DOM structure of `floor-row`:
- `.status-row`: `grid-template-columns: 130px 1fr 90px`
- `.status-cells`: nested grid with `repeat(4, 1fr)` + `border-right: 1px`
- Updated CSS to enforce alignment rule going forward

#### Rule Going Forward
**Any row that must align with shaft columns must use the same two-level grid structure:** outer `130px 1fr 90px`, inner `repeat(4, 1fr)` nested inside the `1fr` slot. Never replicate shaft geometry with flat `repeat(4, 1fr)` at outer level.

#### Outcome
- Each status label centered directly under corresponding shaft column
- Build clean, 13/13 tests passing
- Pure CSS change, no markup logic altered

### 4. Backend Coverage Measurement Scope
- **Date:** 2026-05-11T12:49:21.000-05:00
- **Authors:** Mal (strategy), Wash (implementation), Simon (validation)
- **Status:** Decided

#### Coverage Target & Exclusions
- Measure line coverage for `ElevatorSimulation` and `ElevatorApi` domains
- Exclude generated artifacts (`obj/**`, OpenAPI interception types, source-generator output, Razor/UI assemblies)
- Use `coverlet.collector` + `src/ElevatorTests/coverage.runsettings` to enforce scope

#### Rationale
- Generated OpenAPI support and co-hosted UI distort total coverage metrics, obscuring true backend risk
- This scope keeps measurement focused on shippable backend behavior (simulation engine, API contracts, validation)
- Prevents duplicate UI test churn and focuses team effort on architectural confidence

#### Measurement Command
```
dotnet test src/ElevatorTests --collect:"XPlat Code Coverage;Exclude=[ElevatorUI*]*,[ElevatorApi]Microsoft.AspNetCore.OpenApi.Generated.*,[ElevatorApi]System.Runtime.CompilerServices.*"
```

#### Current Baseline
- **Line coverage: 95.30%** (backend scope)
- **Test count: 42/42 passing**

### 5. Deployment Scaffolding Parity
- **Date:** 2026-05-11T12:58:00.000-05:00
- **Author:** Zoe (DevOps)
- **Status:** Decided

#### Alignment Strategy
Reuse golden repo's (`dadabase.demo`) workflow/template pattern and Bicep module structure, constrained to elevator app's real deploy target (web app hosting `ElevatorApi`)

#### Applied Pattern
- `projects.yml` maps `web` → `src/ElevatorApi`, tests → `src/ElevatorTests`
- `1-deploy-bicep.yml` limits deployment types to `webapp`
- `infra/Bicep/main.bicep` follows golden naming/outputs but deploys only web infrastructure
- `template-webapp-build.yml` resilient when `coverage.runsettings` absent

#### Rationale
- Reduces CI/CD drift and preserves operator familiarity
- App currently has one deployable workload; premature function/container paths add operational risk
- Future: when function/container workloads introduced, extend `projects.yml`, restore deployment types, expand Bicep modules using same template pattern

### 6. GitHub Actions Workflows Documentation
- **Date:** 2026-05-11T16:05:19-05:00
- **Author:** Zoe (DevOps)
- **Status:** Documented

#### Decision
Created `.github/workflows-readme.md` as the canonical guide for setting up and running GitHub Actions deployment workflows in this repo.

#### Rationale
- **Operator clarity:** The concrete workflows (1-deploy-bicep, 2.1-bicep-build-deploy-webapp) and their dependencies need clear documentation.
- **Secret setup:** Teams need explicit instructions for configuring OIDC secrets (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`) at environment level.
- **Reusable templates:** The template workflows are internal plumbing; documenting which ones exist and how they chain together reduces support burden.
- **Consistent tone:** Documentation matches repo conventions — clear, practical, not overly verbose.

#### Documentation Scope
1. **Workflow Overview** — Lists all templates and concrete workflows with one-line descriptions.
2. **Deployment Sequence** — Shows the job order in the 2.1 workflow (Load Config → Infra → Build → Deploy → Smoke Test).
3. **Required Secrets** — OIDC setup instructions (both CLI and Web UI), secret definitions, and OIDC vs. client secret trade-offs.
4. **Custom Login Action** — Explains `.github/actions/login-action` behavior at a high level.
5. **Config & Variables** — Documents `.github/config/projects.yml` mapping and environment variables.
6. **Running Workflows** — Web UI and GitHub CLI examples.
7. **Bicep Deployment Modes** — create / validate / whatIf reference.
8. **Troubleshooting** — Common errors and resolution steps.
9. **Code Coverage** — Notes on test/coverage baseline (95.30%) and scope.
10. **Security Notes** — OIDC preference, credential rotation, least-privilege principle.

#### Updated README
Updated `README.md` to include a "GitHub Actions Deployment" section that:
- Links to the new workflows doc
- Provides quick-start summary
- Guides operators to the detailed guide for full setup

#### Impact
- New operators can self-serve workflow setup without asking the team
- Reduces context-switching for re-deployments
- Establishes a reference point for future workflow changes

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
