# Project Context

- **Owner:** Lyle MS Luppes
- **Project:** Build an in-memory 5-floor, 4-elevator dispatch simulation with live Blazor dashboard and Aspire orchestration.
- **Stack:** C#/.NET 10+, ASP.NET Core API, SignalR, Blazor WebAssembly, xUnit, Azure Aspire
- **Created:** 2026-05-11T10:43:30.058-05:00

## Learnings

- Initial team setup complete from PRD `docs/prd-elevator-dispatch.md`.

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
- Deployment parity established with golden repo patterns: added reusable GitHub workflow templates (`template-load-config`, `template-bicep-deploy`, `template-webapp-build`, `template-webapp-deploy`) plus composite actions/config (`.github/actions/login-action`, `.github/actions/load-project-config`, `.github/config/projects.yml`) wired for ElevatorApi + ElevatorTests.
- Added `infra/Bicep` webapp-focused scaffolding mirroring golden module layout (`main.bicep`, `main.bicepparam`, `resourcenames.bicep`, webapp/iam/monitor modules) and kept naming/token conventions compatible with the shared deploy workflow.
- Validation pass: `az bicep build --file infra/Bicep/main.bicep`, `dotnet build ElevatorDemo.slnx`, and `dotnet test src/ElevatorTests/ElevatorTests.csproj` all completed successfully (existing NuGet vulnerability warnings unchanged).
- **2026-05-11** Team coverage & deployment parity consolidated (Scribe):
   - **Decision #4 (Backend Coverage Measurement Scope):** Standardized team measurement to exclude generated artifacts (OpenAPI types, Razor UI) and focus on ElevatorSimulation/ElevatorApi domains. Baseline: 95.30% line coverage.
   - **Decision #5 (Deployment Scaffolding Parity):** Aligned deployment scaffolding with golden repo patterns (reusable workflows, Bicep modules). Limited to web-app deployment to avoid operational risk.
   - Cross-agent orchestration logs written; decisions merged from team inbox; measurement command documented for reproducibility.
- **2026-05-11** AZD parity pass completed against golden repo (`dadabase.demo`):
  - Added AZD entrypoints (`azure.yaml`, `infra/azd-main.bicep`, `infra/azd-main.parameters.json`) using the same naming/file layout/token style as golden.
  - Preserved golden AZD conventions (`AZURE_ENV_NAME`, `AZURE_LOCATION`, `web` service, App Service host, `.azure/.gitignore` handling).
  - Adapted service/project and infra parameters only where required for Elevator architecture (`src/ElevatorApi`, `appName`, `deploymentType=webapp`).
  - Validation: `azd version`, `az bicep build --file infra/azd-main.bicep`, `az bicep build --file infra/Bicep/main.bicep`, `dotnet build ElevatorDemo.slnx`, `dotnet test src/ElevatorTests/ElevatorTests.csproj` (existing warnings unchanged).
- **2026-05-11** AZD env bootstrap updated for parity:
  - `azure.yaml` now runs a `preup` hook that prompts for `APP_NAME`, `DEPLOYMENT_TYPE`, `ENVCODE`, `INSTANCE_NUMBER`, and `RESOURCE_GROUP_LOCATION` when missing.
  - `infra/azd-main.parameters.json` now maps the azd environment variables directly into the Bicep inputs.
  - Added `infra/Bicep/main.bicepparam` to mirror the golden repo token style and updated `.azure/readme.md` with the override flow.
