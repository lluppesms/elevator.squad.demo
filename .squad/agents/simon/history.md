# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** Build an in-memory 5-floor, 4-elevator dispatch simulation with live Blazor dashboard and Aspire orchestration.
- **Stack:** C#/.NET 10+, ASP.NET Core API, SignalR, Blazor WebAssembly, xUnit, Azure Aspire
- **Created:** 2026-05-11T10:43:30.058-05:00

## Learnings

- Initial team setup complete from PRD `docs/prd-elevator-dispatch.md`.
- Coverage uplift work (2026-05-11): added focused xUnit tests for `SimulationEngine` edge/error paths (tick completion, door countdown, service-floor boarding/drop-off, direction reversal, restart reset, pause behavior, deep snapshot copies).
- API validation tests were already exercising REST endpoints; filtered coverage is now measured with `Exclude=[ElevatorUI*]*,[ElevatorApi]Microsoft.AspNetCore.OpenApi.Generated.*,[ElevatorApi]System.Runtime.CompilerServices.*` to avoid counting UI assembly and generated OpenAPI interception code.
- Current measured line coverage with that scope is 95.12% (ElevatorSimulation 96.33%, ElevatorApi 89.28%, ElevatorServiceDefaults 94.11%).

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
- **2026-05-11** Team coverage & deployment parity consolidated (Scribe):
   - **Decision #4 (Backend Coverage Measurement Scope):** Standardized team measurement to exclude generated artifacts (OpenAPI types, Razor UI) and focus on ElevatorSimulation/ElevatorApi domains. Baseline: 95.30% line coverage.
   - **Decision #5 (Deployment Scaffolding Parity):** Aligned deployment scaffolding with golden repo patterns (reusable workflows, Bicep modules). Limited to web-app deployment to avoid operational risk.
   - Cross-agent orchestration logs written; decisions merged from team inbox; measurement command documented for reproducibility.
