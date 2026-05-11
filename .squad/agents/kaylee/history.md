# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** Build an in-memory 5-floor, 4-elevator dispatch simulation with live Blazor dashboard and Aspire orchestration.
- **Stack:** C#/.NET 10+, ASP.NET Core API, SignalR, Blazor WebAssembly, xUnit, Azure Aspire
- **Created:** 2026-05-11T10:43:30.058-05:00

## Learnings

- Initial team setup complete from PRD `docs/prd-elevator-dispatch.md`.

## UI Refresh [2026-05-11]

**Task:** Remove Blazor template leftovers, add clean app shell, modernize styling.

**Changes made:**
- Deleted: `Counter.razor`, `Weather.razor`, `NavMenu.razor`, `NavMenu.razor.css`
- Rewrote `MainLayout.razor`: sidebar + double-title-bar replaced with single sticky `<header>` + `<main>`
- Rewrote `MainLayout.razor.css`: dark gradient app header, no sidebar styles
- Rewrote `app.css`: full design-token system via CSS custom properties (`--color-*`, `--radius-*`, `--shadow-*`), modernized all dashboard layout classes
- Updated `Home.razor`: removed inline `<h1>` title (now in header), renamed `hero-banner`→`status-bar`, `hero-stats`→`status-stats`, `controls`→`controls-panel`
- Modernized `BuildingView.razor.css` + `MovementSummary.razor.css`: richer elevator cab gradients, hover states, better spacing/radius
- Build: clean (`dotnet build`) — 13/13 tests passing, zero new errors

**Key decisions:**
- App title lives in the persistent `<header>`, not inside page content
- CSS custom properties defined on `:root` in `app.css` flow into scoped component CSS via fallback syntax `var(--token, fallback)`
- Elevator color palette updated to vivid modern tones (green/blue/purple/slate) with gradient fills

## Project Scaffold & Decisions [2026-05-11]

*From Mal (Lead):* Project scaffolded and building cleanly:
- Solution: ElevatorDemo.sln at repo root, all projects under src/
- ElevatorApi co-hosts ElevatorUI via UseBlazorFrameworkFiles() — one service in Aspire
- ElevatorSimulation is a class library; uses Microsoft.Extensions.Hosting.Abstractions for BackgroundService
- Tests use xUnit (13/13 passing) — overrides repo default of MSTest per PRD spec
- Build: dotnet build at root — all 6 projects compile cleanly
- Test: dotnet test src/ElevatorTests — 13/13 pass
- Run: dotnet run --project src/ElevatorAppHost (or spire run from AppHost dir)
- AppHost registers only levator-api; UI is co-hosted from same process
- SimulationSettings configurable via ppsettings.json section "Simulation" (TickIntervalSeconds, SpawnChance, MaxTicks)

**Key Architectural Decision:** Co-hosted Blazor WASM avoids CORS complexity and dynamic port wiring in Aspire.

## UI Shell Completion & Design System [2026-05-11]

**Task:** Complete UI shell refresh with unified header, remove template artifacts, establish design token system.

**Changes applied:**
- Scoped CSS design tokens system in `app.css`: all colours, radii, shadows as custom properties on `:root`
- Component CSS files now reference tokens via `var(--token, fallback)` syntax for safety
- Elevator cab colours updated to vivid modern palette: green/blue/purple/slate with gradient fills
- Status bar replaces hero banner: compact card showing live tick/queued/waiting with status pill
- All page titles centralized in persistent `<header role="banner">` (title + "Live Simulation" tagline)
- MainLayout simplified: single sticky header + main content area (block flow, no sidebar gutter)
- Removed competing NavMenu.razor and cosmetic title bars; recovered 250 px viewport width

**Build & test:** Clean build, 13/13 tests passing

**Design & future paths:**
- Dark mode foundation in place (can add via `@media (prefers-color-scheme: dark)` using same tokens)
- CSS Grid layout deferred; block flow sufficient for single-page app
- Token system ready for theme variations and accessibility polish

**Decisions documented:** Merged kaylee-ui-refresh.md into squad decisions.md for team reference

## Grid Alignment Refinement [2026-05-11]

**Task:** Fix status-row/shaft-column visual misalignment on dashboard.

**Problem diagnosed:**
- Status row labels (elevator IDs) were misaligned with shaft columns above them
- Root cause: flat grid vs. nested grid structure — floor-row uses 2-level grid (outer `130px 1fr 90px` → inner `repeat(4, 1fr)`), but status-headers used flat grid
- Border-right: 1px on shaft-row shrinks content box, causing 1 px cumulative drift per column (~4 px by column 4)
- Visible at certain viewport widths and zoom levels

**Changes applied:**
- Restructured status-row markup to mirror exact 2-level DOM of floor-row
- Added nested .status-cells grid container with `repeat(4, 1fr)` + `border-right: 1px`
- Updated CSS: .status-row outer grid matches floor-row template; .status-cells inner grid matches shaft-row
- Added .shaft-header:last-child { border-right: none; } for visual parity

**Outcome:** Each status label now perfectly centered under its shaft column at all viewport widths. Build clean, 13/13 tests passing.

**Pattern established:** Any row aligning with shaft columns must use the same 2-level grid topology: outer `130px 1fr 90px`, inner `repeat(4, 1fr)`. Documented as team rule in decisions.md.

**Decisions documented:** Merged kaylee-elevator-status-alignment.md + kaylee-status-row-alignment.md into squad decisions.md as decision #3
