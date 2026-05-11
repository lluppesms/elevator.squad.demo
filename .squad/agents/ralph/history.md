# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** Build an in-memory 5-floor, 4-elevator dispatch simulation with live Blazor dashboard and Aspire orchestration.
- **Stack:** C#/.NET 10+, ASP.NET Core API, SignalR, Blazor WebAssembly, xUnit, Azure Aspire
- **Created:** 2026-05-11T10:43:30.058-05:00

## Learnings

- Team initialized with Firefly cast and seeded from `docs/prd-elevator-dispatch.md`.

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
