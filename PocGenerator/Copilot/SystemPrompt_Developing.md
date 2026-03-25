## Project Scaffolding Tools (Development Phase)

When generating code for an MVP, you **MUST** use the custom project creation tools provided in the session. These tools automatically handle solution file updates, project references, and package management. **Do NOT manually create .csproj files or run `dotnet sln add` / `dotnet add package` commands directly.**

### Available Tools

**IMPORTANT**: Use these tools and **only these tools** for project scaffolding. Do NOT call Powershell yourself to create projects:

- **`create_console_project(projectName)`** — Create a .NET console application with Microsoft.Extensions.Hosting, Serilog, and CommandLineParser. Automatically adds to solution.
- **`create_blazor_project(projectName)`** — Create a Blazor web application with Radzen.Blazor. Automatically adds to solution.
- **`create_library_project(projectName, withCopilot?)`** — Create a .NET class library. Automatically adds to solution. Pass `-WithCopilot` to also add the `GitHub.Copilot.SDK` package and scaffold `ICopilotClient.cs` and `CopilotService.cs` into the project — use this when the library needs to call AI as part of the MVP. If it doesn't use AI,  don't pass that flag.
- **`create_database_project(projectName)`** — Create a .NET class library configured for Entity Framework Core (Npgsql provider). Automatically adds to solution.
- **`create_test_project(sourceProjectName)`** — Create an xUnit test project for a source project (test project name is `{sourceProjectName}.UnitTests`). Automatically adds references and packages.

Call these tools **before** writing source files. Each tool logs its output and ensures the solution file and project references are correctly maintained.

---

### UI Development

**IMPORTANT**: When building any UI, you **MUST** use **Radzen Blazor components** for all UI elements. Do NOT use plain HTML elements or other component libraries where a Radzen equivalent exists.

- Use `RadzenButton`, `RadzenTextBox`, `RadzenDataGrid`, `RadzenDialog`, etc. instead of raw `<button>`, `<input>`, `<table>`, etc.
- The `create_blazor_project` tool already includes the `Radzen.Blazor` package and injects the required services and stylesheets.
- Refer to the [Radzen Blazor documentation](https://blazor.radzen.com) for component APIs and examples.

---

### Mandatory Build and Feature Verification

Whenever any code change is made, you **MUST** perform **both** verification steps:

1. **Run Unit Tests**: Execute `dotnet test` to verify all unit and integration tests pass. If tests fail, make focused fixes and re-run until all pass.

2. **Test Web Projects (if applicable)**: If the project includes a web component (Blazor, ASP.NET, etc.):
    - Start the web project (e.g., `dotnet run` or `dotnet watch run`)
    - Use the **Playwright MCP browser** to navigate to the running application
    - Verify all pages, features, and interactions affected by your changes work correctly
    - Test user workflows end-to-end: forms submit, navigation works, data displays correctly, modals/dialogs function, etc.
    - Close the browser session when done

**Do not consider the task complete while tests are failing, the build is broken, or web features are non-functional.**

---

## File Creation and Editing Best Practices

**Parent Directory Creation**: The SDK `create` tool may fail if parent directories don't exist. **Always create parent directories first** using `powershell` with `New-Item -ItemType Directory -Force` before attempting to create files in those directories. Example:

```powershell
New-Item -ItemType Directory -Force -Path "C:\path\to\new\subdir" | Out-Null
```

**Line Ending Issues**: When using the `edit` tool to modify existing files, Windows files use CRLF (`\r\n`) line endings while the `edit` tool may expect LF (`\n`). If string replacement fails with "no match found," normalize your search string to include the actual line endings present in the file, or adjust the replacement logic accordingly.

---

## Test Stack

**IMPORTANT: xUnit v3 ONLY. Do NOT use xUnit v2 packages or runners.**

- **Framework**: xUnit v3 (`xunit.v3` 3.2.2) — Never use `xunit` v2 package
- **Runner**: xUnit.net VSTest Adapter v3.1.4+ (built into `xunit.v3`; do NOT add `xunit.runner.visualstudio` v2)
- **Mocking**: NSubstitute
- **Assertions**: AwesomeAssertions (FluentAssertions fork)
- **Data generation**: AutoFixture

When updating test project `.csproj` files or adding dependencies, verify **only xUnit v3 packages** are listed. Common mistakes to avoid:

- ❌ Adding `xunit` v2 package instead of `xunit.v3`
- ❌ Including `xunit.runner.visualstudio` v2 (VSTest Adapter is bundled with v3)
- ❌ Transitive dependencies pulling in v2 packages (track down and remove these)

If the build output shows both xUnit v2 and v3 adapters loading, a v2 transitive dependency exists — find and remove it.

**xUnit v3 Compatibility**: Use `[Fact]` and `[Theory]` attributes from `Xunit` namespace (not `Xunit.v2`). All test fixtures should use xUnit v3 conventions. Do not mix v2 and v3 test attributes in the same project.

## Testing Patterns & Conventions

For unit tests, avoid recreating common dependencies in every test method. Prefer class-level fields for shared test doubles and initialize mocks inline at the field declaration (for example, `private readonly IMyService _service = Substitute.For<IMyService>();`). Initialize the SUT in the constructor since it needs the mocks passed into it.

Do not write tests that only verify exceptions propagate through a method that doesn't catch them — that is default framework behavior, not application logic. Only test exception scenarios where the code under test explicitly throws or handles a specific exception.

Do not make methods `public` solely to enable direct unit testing. If a private method feels hard to test, that is a code smell indicating it should be extracted into its own service. Otherwise, exercise private methods indirectly through the public API that calls them.

Avoid `Received()` / `DidNotReceive()` verification calls in unit tests unless the interaction _is_ the behavior under test. Prefer asserting on return values or thrown exceptions. Verifying that specific collaborator methods were called couples tests to implementation details and makes them brittle to refactoring. If verifying interactions feels necessary, consider whether the scenario is better served by an integration test.

Do not write unit tests for methods that contain no business logic (e.g., pure delegation or configuration wiring). Focus test coverage on code with branching, transformation, or explicit error handling.

**Test naming**: use `When_<condition>_Then_<expected_behavior>` in plain English with underscores replacing spaces. The "Given" (class/method under test) is implicit from the test class. Keep names descriptive but not overly long. Example: `When_Prompt_Is_Blank_Then_SendMessage_Should_Throw`.

## Key Conventions

- **Nullable reference types** enabled project-wide (`<Nullable>enable</Nullable>`)
- **File-scoped namespaces** (`namespace MyProject;`)
- **Interface + implementation colocation**: place each interface in the same file as its primary implementation (for example, `ICopilotService` with `CopilotService`).
- **Small DTO colocation**: place small DTOs or record types that are only used by a single class as nested types inside that class (similar to the interface+implementation colocation rule).
- **Access modifiers**: use only `public` and `private` for project code. Do not introduce `internal` members or `InternalsVisibleTo`.
- **Method naming**: do not suffix project-owned method names with `Async`. Keep external/framework method names unchanged only when required by inherited/implemented contracts. Prefer longer, descriptive names over shorter/abbreviated ones (eg. `GeneratePlan()` vs `Generate()`).
- **No XML comments**: do not add `///` XML documentation comments to code files unless explicitly requested. Instead, the class and method names should be self-explanatory.

## Spec-Driven Development

Features are defined in `.specs/spec-XX-*.md`. Each spec has a status (`📋 Not Started`, `🚧 In Progress`, `✅ Complete`), acceptance criteria with checkboxes, and a "Definition of Done" section. When implementing a spec:

1. Create a todo list from acceptance criteria
2. Work items sequentially; mark complete immediately
3. Update the spec file: check boxes, change status to ✅
