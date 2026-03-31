# Copilot Instructions — PocGenerator (Overnight POC Factory)

## Project Overview

A .NET 10 console app that takes a SaaS idea description and uses the **GitHub Copilot SDK** to generate a complete, runnable proof-of-concept (POC). The app orchestrates: scaffold → plan → generate → validate → fix loop → finalize.

## Architecture

- **`Program.cs`** — Generic Host entry point; registers all services via DI, including `ConsoleRunner` as a `BackgroundService`.
- **`ConsoleRunner`** — `BackgroundService` that drives the top-level flow: initialize → plan → generate → verify. Delegates to phase handlers; contains no business logic.
- **`ICopilotClient` / `CopilotClientImpl`** — Thin wrapper around `GitHub.Copilot.SDK.CopilotClient` enabling DI and testability. The interface mirrors the SDK surface (start, stop, sessions, messaging). Do **not** call `CopilotClient` directly; always go through `ICopilotClient`.
- **`ICopilotService` / `CopilotService`** — High-level abstraction managing client lifecycle. Exposes `Initialize`, `CreateSession(CreateSessionConfig)`, and `SendMessage(SendMessageConfig)`. `CreateSessionConfig` carries the system prompt and optional output directory; `SendMessageConfig` carries the prompt, session, optional attachment paths, and optional timeout.
- **`ISystemPromptProvider` / `SystemPromptProvider`** — Loads system prompt markdown files from disk for each phase (planning, developing, verification).
- **`IOutputDirectoryService` / `OutputDirectoryService`** — Creates the timestamped output folder, copies project scripts, `Directory.Build.props`, and the `mvp-definition` folder into it.
- **`IProcessRunner` / `ProcessRunner`** — Thin wrapper around `Process` for running shell commands (e.g. `dotnet new`).
- **Planning phase** (`Planning/`):
  - `IPlanningPhaseHandler` / `PlanningPhaseHandler` — Orchestrates the planning flow: locate idea → generate slug → generate plan → split into specs.
  - `IIdeaFileLocator` / `IdeaFileLocator` — Finds `mvp-definition/mvp.md` and any sibling context files.
  - `ISlugGenerator` / `SlugGenerator` — Asks Copilot to produce a short kebab-case project name from the idea file.
  - `IProjectPlanner` / `ProjectPlanner` — Sends the idea to Copilot and writes an `implementation-plan.md` to the output folder.
  - `ISpecSplitter` / `SpecSplitter` — Asks Copilot to split the plan into per-feature spec markdown files (max `SpecSplitter.MaxSpecCount`).
- **Generation phase** (`Generation/`):
  - `IGenerationPhaseHandler` / `GenerationPhaseHandler` — Iterates over spec files and delegates each to `ICodeGenerator`.
  - `ICodeGenerator` / `CodeGenerator` — Creates a fresh Copilot session per spec and sends a generation prompt. Enforces `HardCapConfig.MaxRequests` (default 50) across all specs.
- **Verification phase** (`Verification/`):
  - `IVerificationPhaseHandler` / `VerificationPhaseHandler` — Runs `dotnet build` and `dotnet test` via Copilot (with Playwright MCP for web projects), then generates a README in a second session.

## Build & Run

```powershell
dotnet build PocGenerator
dotnet run --project PocGenerator
```

The app reads the idea from `PocGenerator/mvp-definition/mvp.md` automatically. Use `--output <path>` to override the default output directory (`./mvp-outputs`):

```powershell
dotnet run --project PocGenerator -- --output ./my-outputs
```

### Mandatory Build Verification

Whenever any code change is made, you **must** run:

```powershell
dotnet build PocGenerator
```

If the build fails, continue making focused fixes and re-running `dotnet build PocGenerator` until it succeeds.
Do not consider the task complete while the build is failing.

## Test Stack

- **Framework**: xUnit v3 (`xunit.v3` 3.2.2)
- **Mocking**: NSubstitute
- **Assertions**: AwesomeAssertions (FluentAssertions fork)
- **Data generation**: AutoFixture
- **Coverage**: Coverlet

```powershell
dotnet test PocGenerator.Tests
```

## Testing Patterns & Conventions

Tests mirror the main project's folder structure inside `PocGenerator.Tests/` (e.g. `Copilot/`, `Generation/`, `Planning/`, `Verification/`). Reference the main project and mock interfaces via NSubstitute.

For unit tests, avoid recreating common dependencies in every test method. Prefer class-level fields for shared test doubles and initialize mocks inline at the field declaration (for example, `private readonly ICopilotClient _client = Substitute.For<ICopilotClient>();`). Initialize the SUT in the constructor since it needs the mocks passed into it.

Do not write tests that only verify exceptions propagate through a method that doesn't catch them — that is default framework behavior, not application logic. Only test exception scenarios where the code under test explicitly throws or handles a specific exception.

Do not make methods `public` solely to enable direct unit testing. If a private method feels hard to test, that is a code smell indicating it should be extracted into its own service. Otherwise, exercise private methods indirectly through the public API that calls them.

Avoid `Received()` / `DidNotReceive()` verification calls in unit tests unless the interaction _is_ the behavior under test. Prefer asserting on return values or thrown exceptions. Verifying that specific collaborator methods were called couples tests to implementation details and makes them brittle to refactoring. If verifying interactions feels necessary, consider whether the scenario is better served by an integration test.

Do not write unit tests for methods that contain no business logic (e.g., pure delegation or configuration wiring). Focus test coverage on code with branching, transformation, or explicit error handling.

**Test naming**: use `When_<condition>_Then_<expected_behavior>` in plain English with underscores replacing spaces. The "Given" (class/method under test) is implicit from the test class. Keep names descriptive but not overly long. Example: `When_Prompt_Is_Blank_Then_SendMessage_Should_Throw`.

## Key Conventions

- **Nullable reference types** enabled project-wide (`<Nullable>enable</Nullable>`)
- **Implicit usings** enabled; global `using Xunit;` in the test project
- **File-scoped namespaces** (`namespace PocGenerator;`)
- **Interface + implementation colocation**: place each interface in the same file as its primary implementation (for example, `ICopilotService` with `CopilotService`).
- **Small DTO colocation**: place small DTOs or record types that are only used by a single class as nested types inside that class (similar to the interface+implementation colocation rule).
- **Access modifiers**: use only `public` and `private` for project code. Do not introduce `internal` members or `InternalsVisibleTo`.
- **Method naming**: do not suffix project-owned method names with `Async`. Keep external/framework method names unchanged only when required by inherited/implemented contracts.
- **No XML comments**: do not add `///` XML documentation comments to code files unless explicitly requested. Instead, the class and method names should be self-explanatory.
- Logging via **Serilog** (configured in `Program.cs` via `.UseSerilog()`)

## Spec-Driven Development

Features are defined in `.github/specs/SPEC-XX-*.md`. Each spec has a status (`📋 Not Started`, `🚧 In Progress`, `✅ Complete`), acceptance criteria with checkboxes, and a "Definition of Done" section. When implementing a spec:

1. Create a todo list from acceptance criteria
2. Work items sequentially; mark complete immediately
3. Update the spec file: check boxes, change status to ✅

See `.github/skills/spec-execution/SKILL.md` for full workflow. New specs follow the template in `.github/skills/spec-creation/SKILL.md`.

## SDK Integration Notes

### Copilot SDK Documentation, Usage, and Examples

Whenever you need to look up anything about the Copilot SDK (for example: types, exposed members, method signatures, or correct usage patterns), first check this project for an example.

**IMPORTANT**: If no example exists in this project, DO NOT try to use reflection or similar strategies to discover if types/classes/members/etc exist in the Copilot SDK package. Instead, you **MUST** lookup the official Copilot SDK documentation:

- Find types, examples, and general documentation here: https://github.com/github/copilot-sdk/blob/main/dotnet/README.md
- Here are some other docs/guides, usually for specific topics if the above link is not sufficient: https://github.com/github/copilot-sdk/tree/main/docs

Prefer these sources before attempting decompilation, as they are usually faster and more reliable for discovering accurate SDK usage.

The `GitHub.Copilot.SDK` communicates via JSON-RPC with a local Copilot CLI server. Key flow:

1. `StartAsync()` — spins up or connects to CLI server
2. `CreateSessionAsync(config)` — establishes a session (model, tools, hooks)
3. Send messages through the session, receive responses
4. `StopAsync()` — tears down sessions and connection

## Generation Pipeline

The full pipeline is implemented across three phase handlers:

1. **Planning** (`PlanningPhaseHandler`) — Locates `mvp-definition/mvp.md`, generates a project slug, creates an `implementation-plan.md`, and splits it into per-feature spec files in a `Specs/` subdirectory.
2. **Generation** (`GenerationPhaseHandler` → `CodeGenerator`) — Iterates over spec files; opens a fresh Copilot session per spec with the developing system prompt and `ProjectTools` (scaffold helpers). Includes a hard cap of 50 Copilot requests total.
3. **Verification** (`VerificationPhaseHandler`) — Opens a session with the verification system prompt and Playwright MCP; runs `dotnet build`/`dotnet test`, exercises the UI for web apps, then generates a README in a second session.
