# Product Requirements Document: Elevator Dispatch System

## Document Control

- File name: `docs/prd-elevator-dispatch.md`
- Owner: Workshop facilitator
- Stakeholders: Workshop participants, facilitator
- Status: Approved
- Created: 2026-05-09
- Last updated: 2026-05-12
- Target release or lab milestone: Labs 01-04 complete; Labs 02.03-02.06 persistence and reset workflows; Lab 03.01 PR review prompts; Azure migration preparation

## Summary

Build an elevator dispatch simulation for a 5-floor, 4-elevator building, exposed through a C# backend API and visualized in a real-time Blazor WebAssembly dashboard. The live simulation state remains in memory, while an optional SQL Server/Azure SQL persistence path records simulation runs and passenger lifecycle events when `DatabaseConnectionString` is configured. The project serves as a hands-on workshop starter that teaches modular C# design, state management, async SignalR communication, simple scheduling heuristics, and optional database-backed event logging, all running within an Azure Aspire orchestration environment.

The repository should also function as a workshop lab environment. Participants need a clear setup path, prerequisite checklist, repeatable validation commands, and discoverable Copilot customizations for prompts, skills, agents, path-specific instructions, issue metadata, PR review workflows, and Azure migration guidance.

For demonstration resets, the repository may include a top-level `completed/` folder containing a facilitator reference solution copied from `workspace/`. That folder is intentionally excluded from Copilot context during rebuild labs so participants can recreate the solution from indexed prompts and visible project assets instead of using the completed implementation as source material.

## Dashboard Target State

The screenshot below shows the desired state of the live
dashboard after Lab 01 is complete.

![Dashboard target state](images/live-dashboard-target-state.png)

## Workshop Setup and Prerequisites

### Must-Have Now

| Requirement | Notes |
| --- | --- |
| GitHub account | Required to fork, clone, or open the assigned workshop repository. |
| GitHub Copilot access | Required for Copilot Chat, agent mode, prompt files, and skill-driven workflows. |
| VS Code | Primary workshop editor with GitHub Copilot and Copilot Chat extensions enabled. |
| Git | Required for local clone, branch, commit, and pull request workflows. |

### Supported Setup Paths

| Path | Target User | Required Runtime |
| --- | --- | --- |
| GitHub Codespaces | Workshop participants who need the fastest setup | Repository devcontainer builds the runtime and tools. |
| VS Code Dev Containers | Participants using local containers | Docker Desktop or compatible container engine plus Dev Containers extension. |
| Manual setup | Participants who cannot use containers | .NET SDK 10.0+, Git, and optional SQL Server or Azure SQL access. |

### Permissions and Policy Requirements

- Most labs require repository write access and any active Copilot license.
- Labs that use GitHub Actions, Copilot coding agent, organization-managed Copilot settings, or cloud deployment tooling may require additional organization permissions.
- Participants in managed enterprise environments should confirm that Codespaces, Copilot agent mode, MCP servers, GitHub Actions, and required marketplace extensions are allowed before the workshop.

### Pre-Configured Developer Tooling

The preferred Codespaces path should provide these tools through `.devcontainer/devcontainer.json`:

| Tooling Area | Expected Capability |
| --- | --- |
| .NET SDK | .NET SDK 10.0+ runtime, NuGet packages restored. |
| Aspire | Aspire CLI and orchestration tooling for local development. |
| GitHub | GitHub CLI, Copilot extensions, pull request and GitHub Actions extensions. |
| Containers | Docker-in-Docker for containerization exercises. |
| Database | SQL Server or Azure SQL sidecar, `DatabaseConnectionString`, init SQL, and client tools. |
| Cloud/IaC | Azure CLI, Azure Developer CLI (`azd`), Bicep support. |
| Agent tooling | MCP Inspector support and repository-local Copilot prompts, skills, instructions, and agents. |

## Problem Statement

Workshop participants need a well-structured, educational
codebase that demonstrates real-time full-stack .NET development
with Blazor and Aspire. Existing elevator simulations are either
too trivial (no UI) or too complex (ML-based schedulers, databases).
This project fills the gap with a clear domain model, straightforward
dispatch heuristic, and a live Blazor dashboard that participants
can extend in subsequent lab steps.

## Goals

- Provide a running 5-floor, 4-elevator simulation out of the box after a single `aspire run` command.
- Render a live building view with animated elevator cabs, passenger dots, floor-level metadata, per-cab movement totals, and average passenger wait time using Blazor Components.
- Keep the codebase small, readable, and easy to extend in a workshop setting.
- Demonstrate async C# patterns, real-time SignalR communication, Blazor WebAssembly components, and Azure Aspire orchestration.
- Demonstrate optional Entity Framework Core integration for SQL Server/Azure SQL event persistence without making the database required for baseline use.
- Provide workshop-grade setup documentation, validation commands, and troubleshooting guidance modeled on a hands-on lab experience.
- Include reusable Copilot assets that demonstrate prompts, skills, instructions, agents, and repeatable verification workflows.
- Demonstrate how small UI changes can be reviewed with GitHub Copilot Review-agent prompts through a branch and pull request workflow.
- Support a facilitator workflow where `workspace/` can be moved to `completed/` and rebuilt from indexed prompts while preserving the completed solution as excluded reference material.

## Non-Goals

- Making database persistence required for the core simulation.
- Using the `completed/` reference solution as source context for Copilot during rebuild labs.
- Authentication or authorization.
- ML-based or optimization-library dispatch algorithms.
- Blazor Server or Multi-tenant support.
- Complex state management libraries (Redux, etc.).

## Users and Personas

| Persona | Needs | Success Looks Like |
| --- | --- | --- |
| Workshop participant | A starter codebase to learn from and extend | Can run the app, see the dashboard, and modify dispatch logic within a lab session |
| Workshop facilitator | A reliable demo with clear extension points | Can walk through code modules, explain heuristics, and assign incremental lab tasks |

## Use Cases

### Use Case 1: Run the Simulation

- Actor: Participant
- Trigger: Starts the Aspire orchestration environment
- Preconditions: .NET SDK installed, NuGet dependencies restored
- Main flow:
  1. Navigate to the project root and run `dotnet build`.
  2. Run `aspire run` to start the orchestration.
  3. Open the dashboard URL provided (typically `http://localhost:17043` for Aspire dashboard).
  4. Observe elevators moving, passengers spawning, and
     the Blazor dashboard updating in real time.
- Alternate or error flows:
  - Port in use: Aspire assigns an alternate port and reports it in console output.
- Outcome: Live Blazor dashboard renders the building view with
  elevator cabs, passenger dots, and status indicators.

### Use Case 2: Add a Passenger Manually

- Actor: Participant
- Trigger: Submits the Add Passenger form in the UI
- Preconditions: Simulation is running
- Main flow:
  1. Select an origin floor and a destination floor.
  2. Click "Add passenger".
  3. The dispatcher assigns the passenger to an elevator.
  4. A status message confirms the assignment.
- Alternate or error flows:
  - Same origin and destination: API returns 400.
  - All elevators full: passenger is queued; status
    message explains the situation.
- Outcome: Passenger appears as a waiting dot on the
  origin floor and boards when an elevator arrives.

### Use Case 3: Pause and Resume

- Actor: Participant
- Trigger: Clicks "Pause simulation" / "Resume simulation"
- Preconditions: Simulation is running
- Main flow:
  1. Click "Pause simulation".
  2. Tick counter stops; elevators freeze in place.
  3. Click "Resume simulation".
  4. Ticks resume; elevators continue moving.
- Outcome: Participant can inspect building state at a
  point in time.

### Use Case 4: Max Ticks Reached and Restart

- Actor: Participant
- Trigger: Simulation reaches 1 000 ticks
- Preconditions: Simulation is running
- Main flow:
  1. Tick counter reaches 1 000.
  2. Simulation auto-pauses and sets `finished` to true.
  3. Status message reads "Simulation complete — maximum of 1 000 ticks reached."
  4. UI shows an alert banner and a "Restart simulation" button.
  5. Participant clicks "Restart simulation".
  6. All state resets to initial values; tick resumes from 0.
  7. If SQL Server/Azure SQL persistence is enabled, application tables are cleared before the fresh run row is created.
- Outcome: A fresh simulation begins with no leftover passengers, elevator state, or previous persisted event history.

### Use Case 5: Persist Passenger Events for Inspection

- Actor: Participant
- Trigger: Starts the app with `DatabaseConnectionString` configured.
- Preconditions: SQL Server/Azure SQL is running and the schema has been initialized.
- Main flow:
  1. Start `aspire run` with `DatabaseConnectionString=Server=localhost;Database=ElevatorDispatch;...`.
  2. Simulation runs and publishes to the Blazor dashboard.
  3. Open Azure Data Studio or SQL Server Management Studio and query `SimulationRuns` and `PassengerEvents` tables.
  4. Observe `Created`, `Assigned`, `Boarded`, and `Exited` records.
- Alternate or error flows:
  - `DatabaseConnectionString` absent: the app runs in memory and skips persistence.
  - Database unavailable: persistence operations fail without crashing the simulation.
- Outcome: Participants can inspect database-backed run history and passenger lifecycle events for future analytics labs.

### Use Case 6: Reset Database Tables for a Clean Demo

- Actor: Participant or facilitator
- Trigger: Runs the reset-all-tables prompt or clicks **Restart simulation** in the UI.
- Preconditions: SQL Server/Azure SQL is running.
- Main flow:
  1. Verify expected tables exist.
  2. Execute `DELETE FROM PassengerEvents; DELETE FROM SimulationRuns;` (preserving schema).
  3. Confirm the simulation is paused.
  4. Confirm row counts are reset.
- Alternate or error flows:
  - App is still running: a fresh `SimulationRuns` row or new passenger events may appear immediately after restart.
- Outcome: Database state is aligned with the visible simulation lifecycle.

### Use Case 7: Review a Small UI Color Change PR

- Actor: Participant
- Trigger: Creates a branch and pull request that changes a single elevator cab color.
- Preconditions: Repository has a Review-agent prompt under `.github/prompts/` and the participant can open pull requests.
- Main flow:
  1. Create a feature branch for the cab color change.
  2. Modify CSS in `ElevatorUI/Components/` for a cab color (e.g., green to teal).
  3. Commit and push the feature branch.
  4. Open a pull request with a focused change description.
  5. Invoke the GitHub Copilot Review agent with the corresponding `03.01` prompt.
- Alternate or error flows:
  - Review finds unrelated code changes: participant narrows the PR to the intended CSS change.
  - Review finds contrast or readability issues: participant adjusts the color or text treatment.
- Outcome: Participant practices using Copilot Review for a small, scoped UI pull request.

## Functional Requirements

| ID | Requirement | Priority | Notes |
| --- | --- | --- | --- |
| FR-001 | The building shall have exactly 5 floors (1–5). | Must | Hardcoded default |
| FR-002 | The building shall have exactly 4 elevators (ev-01 through ev-04). | Must | Default start floors: 1, 2, 3, 4 |
| FR-003 | Each elevator shall have a max capacity of 8 passengers. | Must | |
| FR-004 | Each elevator shall move at most one floor per simulation tick. | Must | |
| FR-005 | Elevator doors shall open for one tick when servicing a floor. | Must | |
| FR-006 | The dispatcher shall assign passengers using a nearest-compatible-cab heuristic. | Must | Idle or same-direction elevators preferred |
| FR-007 | If no compatible elevator exists, the passenger shall be queued until one becomes available. | Must | Pending passenger list in Building |
| FR-008 | Passengers shall spawn randomly each tick based on a configurable spawn chance (default 0.3). | Must | |
| FR-009 | Users shall be able to add passengers manually via an API endpoint and the UI form. | Must | POST `/api/passengers` |
| FR-010 | The API shall validate that origin ≠ destination and both floors are 1–5. | Must | Returns 400 on invalid input |
| FR-011 | The API shall expose a SignalR hub at `/buildinghub` that streams building snapshots. | Must | |
| FR-012 | The UI shall render a live building view with elevator cabs and passenger dots. | Must | |
| FR-013 | Each elevator shall track a `PassengersMoved` counter incremented on drop-off. | Must | |
| FR-014 | The building shall compute a rolling `average_passenger_wait_time_seconds` refreshed every 60 simulation-seconds. | Must | |
| FR-015 | The UI shall display per-cab movement totals, total passengers moved, and average wait time below the building view. | Must | Movement summary section |
| FR-016 | The UI shall show floor-level metadata (waiting count, elevator status) beside each floor row. | Should | |
| FR-017 | The simulation shall support pause and resume via POST `/api/control`. | Must | |
| FR-018 | The UI shall show dotted gridlines on the shaft grid to delineate cells. | Should | |
| FR-019 | The simulation shall stop after 1 000 ticks, auto-pause, set `finished` to true, and display a completion message. | Must | `MAX_TICKS = 1000` |
| FR-020 | The UI shall show an alert banner and a "Restart simulation" button when the simulation finishes. | Must | |
| FR-021 | POST `/api/restart` shall reset all simulation state to initial values and resume ticking from 0. | Must | |
| FR-022 | Each elevator cab shall have a distinct color: ev-01 green, ev-02 blue, ev-03 purple, ev-04 medium grey. | Must | Applied via CSS class per cab |
| FR-023 | The repository README shall provide tutorial-style setup paths, prerequisites, validation commands, repository tour, and troubleshooting. | Must | Modeled on hands-on lab structure |
| FR-024 | The repository shall include reusable Copilot prompts and skills for repeatable lab operations. | Should | Prompt and skill files under `.github/` |
| FR-025 | The devcontainer shall provide optional SQL Server schema inspection support without making persistence mandatory. | Should | SQL Server sidecar, init SQL, and client tools |
| FR-026 | When `DatabaseConnectionString` is set, the simulation shall write run metadata to `SimulationRuns`. | Must | Optional persistence path |
| FR-027 | When `DatabaseConnectionString` is set, passenger lifecycle events shall be written to `PassengerEvents`. | Must | Events: `Created`, `Assigned`, `Boarded`, `Exited` |
| FR-028 | POST `/api/restart` shall clear SQL Server application tables before creating the fresh run row. | Must | Applies only when persistence is enabled |
| FR-029 | The repository shall include prompt files for table reset, reset-on-restart, GitHub issue-type discovery, PR review, and Azure migration preparation. | Should | Prompts `02.05`, `02.06`, `03.00`, `03.01`, `04.00`, `04.01` |
| FR-030 | Azure deployment conventions shall be captured in path-scoped instructions for `workspace/**`. | Should | `.github/instructions/azure-deployment.instructions.md` |
| FR-031 | The repository shall include GitHub Copilot Review-agent prompts that require a branch, focused change, pull request, and scoped review criteria. | Should | Cab color prompt variants under `03.01` |
| FR-032 | The repository documentation shall explain the `completed/` folder as an excluded facilitator reference solution for rebuild labs. | Should | Do not treat as Copilot source context |

## Non-Functional Requirements

| ID | Category | Requirement | Target |
| --- | --- | --- | --- |
| NFR-001 | Performance | Tick loop runs once per second without blocking the event loop. | 1 s tick interval |
| NFR-002 | Reliability | SignalR reconnection is handled by the client. | Auto-reconnect within 2 s |
| NFR-003 | Maintainability | Modules stay under 200 lines each. | All current modules comply |
| NFR-004 | Accessibility | Floor labels and elevator IDs use semantic HTML text. | Screen-reader friendly labels |
| NFR-005 | Portability | Runs on .NET 10.0+ with no OS-specific dependencies. | Windows, macOS, Linux |
| NFR-006 | Onboarding | New participants can select a setup path and validate the environment from README instructions. | 15 minutes or less for Codespaces |
| NFR-007 | Workshop repeatability | Setup and validation commands are scriptable and documented. | Commands work in Codespaces and devcontainer paths |
| NFR-008 | Resilience | Database persistence must not block or crash the simulation when unavailable. | In-memory fallback |
| NFR-009 | Operability | Database reset workflows preserve schema and constraints. | Delete rows, do not drop schema |

## User Experience Requirements

- Primary screens: Single-page Blazor WASM application at `/`.
- Required states: connecting, running, paused, finished.
- Content: Hero banner with tick and queued counts, origin/
  destination selectors, Add passenger and Pause buttons,
  live building view, movement summary row, average wait
  time row, status message.
- Accessibility: Keyboard-navigable form controls, high-
  contrast text on light background, `aria-hidden` on
  decorative passenger dots.
- Responsive: Fluid Blazor component layout using CSS Grid/Flexbox;
  responsive typography with media queries.

## Data Requirements

- Entities: `Building`, `Elevator`, `Passenger`; optional SQL Server/Azure SQL tables for persistence and future analytics labs.
- Required fields:
  - Passenger: `Id`, `OriginFloor`, `DestinationFloor`, `RequestedTick`, `Direction` (derived).
  - Elevator: `Id`, `CurrentFloor`, `Direction`, `DoorState`, `Capacity`, `Passengers`, `ScheduledStops`, `DoorTicksRemaining`, `PassengersMoved`.
  - Building: `FloorCount`, `Elevators`, `WaitingPassengers`, `PendingPassengers`, `Tick`, `Paused`, `StatusMessage`, `TotalPassengerWaitTimeSeconds`, `BoardedPassengerCount`, `AveragePassengerWaitTimeSeconds`, `WaitTimeUpdatedTick`.
- Data lifecycle: Core simulation state is in-memory and resets on server restart or `POST /api/restart`. SQL Server rows are written only when `DatabaseConnectionString` is set. `POST /api/restart` removes rows from `PassengerEvents` and `SimulationRuns`, then prepares a fresh run row.
- Validation: Floor numbers 1–5, origin ≠ destination.
- Seed data: Elevators start at floors 1–4. No passengers at boot.
- Optional SQL Server/Azure SQL tables:
  - `SimulationRuns`: run-level metadata, dispatcher strategy, tick interval, spawn chance, movement totals, and wait time statistics.
  - `PassengerEvents`: passenger lifecycle events with event type (`Created`, `Assigned`, `Boarded`, `Exited`), timestamps, and floor information.
  - `Scenarios`: named scenario rows for future replay labs.
- Privacy: No PII collected.

## API and Integration Requirements

- `GET /` — Serves the Blazor WASM application.
- `GET /api/state` — Returns a full building snapshot (JSON).
- `POST /api/passengers` — Creates a passenger
  (`{ originFloor, destinationFloor }`). Returns 201 on
  success, 400 on validation failure.
- `POST /api/control` — Sets pause state
  (`{ paused: bool }`).
- `POST /api/restart` — Resets all simulation state and
  resumes ticking from 0. Returns the fresh snapshot.
- `SignalR Hub /buildinghub` — Streams building snapshot updates each tick.
  Blazor components subscribe via `HubConnection` for real-time updates.
- Static Blazor WASM assets served automatically.
- Configuration: `TickInterval`, `SpawnChance`, and
  `MaxTicks` are injected via `IOptions<SimulationSettings>` or
  constructor parameters on `SimulationEngine`.
- No external services or databases.

## Technical Approach

Keep simulation logic in the `ElevatorSimulation` project, API logic in the `ElevatorApi` project, Blazor UI in the `ElevatorUI` project, and tests in the `ElevatorTests` project. All state lives in memory inside a single `SimulationEngine` instance protected by async locks in C#. Optional database persistence is handled via Entity Framework Core with SQL Server or Azure SQL, writing to `SimulationRuns` and `PassengerEvents` tables when `DatabaseConnectionString` is configured.

### Proposed Components

| Component | Responsibility | Files or Location |
| --- | --- | --- |
| Passenger | Domain object with origin, destination, direction | `ElevatorSimulation/Models/Passenger.cs` |
| Elevator | Cab state, movement, boarding, drop-off | `ElevatorSimulation/Models/Elevator.cs` |
| Building | Floor queues, pending list, snapshots, wait-time aggregation | `ElevatorSimulation/Models/Building.cs` |
| Dispatcher | Nearest-compatible-cab heuristic, pending retry | `ElevatorSimulation/Services/Dispatcher.cs` |
| SimulationEngine | Tick loop, spawn, publish, async lock | `ElevatorSimulation/Services/SimulationEngine.cs` |
| API Server | Routes, validation, SignalR hub, dependency injection | `ElevatorApi/Program.cs`, `ElevatorApi/Hubs/BuildingHub.cs` |
| Dashboard UI | Blazor components, CSS, interactivity | `ElevatorUI/Components/` |
| Tests | xUnit suite for spawn, dispatch, metrics | `ElevatorTests/` |

### Data Model

```text
Building
├── FloorCount: int = 5
├── Elevators: List<Elevator>  (4 cabs)
├── WaitingPassengers: Dictionary<int, List<Passenger>>
├── PendingPassengers: List<Passenger>
├── Tick: int
├── Paused: bool
├── StatusMessage: string
├── TotalPassengerWaitTimeSeconds: double
├── BoardedPassengerCount: int
├── AveragePassengerWaitTimeSeconds: double
└── WaitTimeUpdatedTick: int

Elevator
├── Id: string
├── CurrentFloor: int
├── Direction: "Up" | "Down" | "Idle"
├── DoorState: "Open" | "Closed"
├── Capacity: int = 8
├── Passengers: List<Passenger>
├── ScheduledStops: HashSet<int>
├── DoorTicksRemaining: int
└── PassengersMoved: int

Passenger
├── Id: string           (auto "psg-NNNN")
├── OriginFloor: int
├── DestinationFloor: int
├── RequestedTick: int
└── Direction: string    (derived property)
```

### Key Flows

```text
Server start (via Aspire)
  → Dependency Injection container initialized
  → SimulationEngine created (4 elevators, 5 floors)
  → BackgroundService.ExecuteAsync() starts tick loop
  → SignalR hub registered for real-time updates

Each tick
  → engine.Tick() acquires SemaphoreSlim lock
  → Increment building.Tick
  → Dispatcher retries pending passengers
  → Advance each elevator (move / service floor)
  → Maybe spawn a random passenger
  → Maybe refresh average wait time (every 60 sim-seconds)
  → Publish snapshot to all SignalR subscribers via BuildingHub

Add passenger (manual)
  → POST /api/passengers validates input
  → engine.AddPassenger() acquires lock
  → Passenger added to floor queue
  → Dispatcher assigns or queues
  → Snapshot broadcast via SignalR

Service a floor
  → Elevator opens doors (1 tick)
  → Drop off arriving passengers (PassengersMoved++)
  → Board compatible waiting passengers
  → Record boarding wait time
  → Close doors, update direction
```

## Acceptance Criteria

- [x] AC-001: Given a fresh server start, when a user
  opens `/`, then the dashboard renders 5 floor rows and
  4 elevator shafts.
- [x] AC-002: Given a running simulation, when a tick
  fires, then each elevator moves at most one floor.
- [x] AC-003: Given a passenger request with origin 3 and
  destination 5, when the dispatcher runs, then the
  closest idle or same-direction elevator is assigned.
- [x] AC-004: Given all elevators at capacity, when a
  passenger is added, then the passenger is queued and the
  status message indicates capacity is full.
- [x] AC-005: Given passengers exit an elevator, when the
  floor is serviced, then `PassengersMoved` increments.
- [x] AC-006: Given passengers have boarded, when 60
  simulation-seconds elapse, then
  `AveragePassengerWaitTimeSeconds` is refreshed.
- [x] AC-007: Given the UI is connected, when state
  changes, then the movement summary displays per-cab
  totals, overall total, and average wait time.
- [x] AC-008: Given the user clicks "Pause simulation",
  when the next tick fires, then the tick counter does not
  advance.
- [x] AC-009: Given the simulation is running, when tick
  reaches 1 000, then the simulation auto-pauses,
  `IsFinished` is true, and the status message says
  "Simulation complete".
- [x] AC-010: Given the simulation has finished, when the
  user clicks "Restart simulation", then all state
  resets and ticking resumes from 0.

## Testing Strategy

- Unit tests: Passenger spawn probability, dispatcher
  assignment and queuing, `PassengersMoved` counter,
  average wait time refresh timing.
- Integration tests: Not required for Lab 01 (in-memory
  only).
- Manual validation:

  ```bash
  dotnet restore
  dotnet build
  dotnet test ElevatorTests/ --verbosity normal
  aspire run
  ```

- Test data: Inline test fixtures using xUnit [Theory] attributes.
- Regression risks: Dispatch heuristic changes may affect
  elevator selection order; wait-time math depends on
  tick-interval alignment.

## Dependencies

- Internal: `ElevatorSimulation`, `ElevatorApi`, `ElevatorUI`, `ElevatorTests`.
- External packages: `Aspire.Hosting >=9.0,<10.0`,
  `Aspire.Hosting.AppHost >=9.0,<10.0`,
  `Microsoft.AspNetCore.Components.WebAssembly >=8.0,<9.0`,
  `Microsoft.AspNetCore.SignalR.Client >=8.0,<9.0`.
- Dev tooling: .NET SDK 10.0+, Aspire CLI, xUnit.

## Risks and Mitigations

| Risk | Impact | Likelihood | Mitigation |
| --- | --- | --- | --- |
| Random spawning makes demos non-deterministic | Medium | High | Configurable `spawn_chance`; set to 0.0 for controlled demos |
| In-memory state lost on restart | Low | Certain | Intentional for workshop simplicity; document in README |
| WebSocket disconnect during demo | Medium | Low | Client auto-reconnects; status message indicates state |

## Open Questions

- [x] Should the wait-time metric use a rolling window or
  cumulative average? — Decision: cumulative average,
  refreshed every 60 simulation-seconds.
- [x] Should the dispatcher use load balancing across
  elevators? — Decision: not for Lab 01; keep the
  nearest-compatible heuristic simple for the workshop.

## Decisions

| Date | Decision | Rationale | Owner |
| --- | --- | --- | --- |
| 2026-05-09 | Cumulative average for wait time | Simpler to implement and explain; rolling window deferred to a future lab | Facilitator |
| 2026-05-09 | No load balancing in dispatcher | Keeps the heuristic easy to read and extend in subsequent labs | Facilitator |
| 2026-05-09 | 60-second refresh interval | Balances update frequency with meaningful sample size | Facilitator |

## Implementation Plan

1. Scaffold .NET project structure using Aspire templates.
2. Implement `Passenger`, `Elevator`, `Building` domain
   objects as C# classes with records for value types.
3. Implement `Dispatcher` with nearest-compatible-cab
   heuristic.
4. Implement `SimulationEngine` with async tick loop,
   spawn logic, and wait-time tracking using BackgroundService.
5. Implement ASP.NET Core API with REST endpoints and SignalR hub.
6. Build Blazor WebAssembly dashboard with components for building view,
   movement summary, and controls.
7. Write xUnit test suite covering spawn, dispatch, metrics.
8. Validate: `dotnet build`, `dotnet test`, manual browser check,
   and `aspire run`.

## Appendix

- Target state screenshot:
  [live-dashboard-target-state.png](images/live-dashboard-target-state.png)
- Initialization prompt:
  `.github/prompts/01.00.initialize-project.prompt.md`
- Copilot instructions:
  `.github/copilot-instructions.md`
