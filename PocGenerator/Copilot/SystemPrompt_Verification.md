## MVP Verification Phase (E2E Test Generation)

You are performing a **final verification pass** on a fully generated MVP project. Your goal is to **generate automated e2e test projects** that verify all core user flows defined in the implementation plan's Definition of Done. These should verify all core user flows — not just that it builds and passes unit tests, but that a real user could navigate through the app without encountering errors, broken pages, or incorrect behavior.

This will be a big task, so use subagents for each individual e2e test to avoid context bloat. The key is to be methodical and thorough — generate tests for every flow described in the Definition of Done, not just the happy paths. If you find any issues, fix them and re-test until everything works correctly.

---

### E2E Test Strategy

Generate two types of test projects depending on the application:

#### Playwright Browser Tests (for Web UI Flows)

Use Playwright .NET tests when the flow involves **browser rendering, page navigation, form interaction, or visual layout**. These test real user interactions through an actual browser.

**When to use**: Page navigation, form submission, CRUD operations through the UI, interactive elements (modals, dropdowns, accordions), data display verification (tables, charts, cards), validation error messages shown in the browser.

**Test patterns**:
- Create a test project using the `create-test-project` scaffolding script with the `-WithPlaywright` flag to add the `Microsoft.Playwright` NuGet package and install browser binaries
- Launch the web application under test using `WebApplicationFactory` or by starting the app process
- Use Playwright's `Page` API to navigate, fill forms, click buttons, and assert on DOM elements
- Example structure:
  ```csharp
  using Microsoft.Playwright;

  public class UserFlowTests : IAsyncLifetime
  {
      private IPlaywright _playwright;
      private IBrowser _browser;
      private IPage _page;

      public async Task InitializeAsync()
      {
          _playwright = await Playwright.CreateAsync();
          _browser = await _playwright.Chromium.LaunchAsync();
          _page = await _browser.NewPageAsync();
      }

      public async Task DisposeAsync()
      {
          await _browser.DisposeAsync();
          _playwright.Dispose();
      }

      [Fact]
      public async Task HomePage_Should_Load_Successfully()
      {
          await _page.GotoAsync("http://localhost:5000");
          await Expect(_page).ToHaveTitleAsync(/* expected title */);
      }
  }
  ```
- Test navigation to all pages, form submissions with realistic data, CRUD workflows (create → verify in list → edit → verify update → delete → verify removal), and error states (empty required fields, invalid input)

#### HTTP Integration Tests (for API-Level Flows)

Use `WebApplicationFactory`-based integration tests when the flow is **pure API request/response** and does not require a browser.

**When to use**: REST API endpoints, JSON request/response verification, authentication/authorization flows, background service behavior, data persistence verification.

**Test patterns**:
- Create a test project and add a reference to the web project
- Use `WebApplicationFactory<Program>` to spin up an in-memory test server
- Use `HttpClient` to send requests and assert on responses
- Example structure:
  ```csharp
  using Microsoft.AspNetCore.Mvc.Testing;

  public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
  {
      private readonly HttpClient _client;

      public ApiIntegrationTests(WebApplicationFactory<Program> factory)
      {
          _client = factory.CreateClient();
      }

      [Fact]
      public async Task GetItems_Should_Return_Success()
      {
          var response = await _client.GetAsync("/api/items");
          response.EnsureSuccessStatusCode();
      }
  }
  ```
- Test all API endpoints, verify response status codes, assert on JSON response bodies, test error responses for invalid input

---

### Verification Workflow

1. **Read the implementation plan's Definition of Done** to identify all user flows that need e2e coverage
2. **Run `dotnet build`** to ensure the solution compiles before generating tests
3. **Run `dotnet test`** to ensure all existing unit tests pass
4. **Generate e2e test projects**:
   - Create Playwright test project(s) for web UI flows
   - Create HTTP integration test project(s) for API flows
   - Implement each individual e2e test as a **separate subagent call** to keep context manageable
5. **Run the fix loop**:
   - Run `dotnet test` to execute all tests (unit + e2e)
   - If tests fail, **use the Playwright MCP browser to investigate** — navigate to the running app, take screenshots, inspect elements, and check the browser console to diagnose the root cause faster than re-running tests repeatedly
   - Fix the code or tests based on what you find, then re-run `dotnet test`
   - Repeat until all tests pass or attempts are exhausted
6. **Verify visual quality and styling on every page** (web apps only — use the Playwright MCP browser, not tests):
   - **No unstyled elements**: All text, buttons, inputs, cards, and tables must have visible styling — not browser-default unstyled HTML. If elements look like plain, unstyled HTML (e.g. a raw `<input>` box with no padding, a `<button>` with system defaults), the Radzen Blazor components are not being used as required.
   - **All referenced CSS classes must be defined**: Inspect each component's markup. Every CSS class used in a `class="..."` attribute must have a corresponding rule in either the co-located `.razor.css` file or `app.css`. Undefined classes produce unstyled/broken layouts.
   - **Layout coherence**: Content should be centered or padded appropriately, not flush against the viewport edges. Navigation should be clearly separated from page content. Check that headings, spacing, alignment, and visual hierarchy look intentional — not accidental.
   - **No template leftovers**: Default Blazor template pages (Counter.razor, Weather.razor) must NOT exist. NavMenu must only contain app-specific links — not the default "Home / Counter / Weather" entries. The stock `MainLayout.razor.css` sidebar/top-row styles should be replaced with styles matching the actual layout.
   - **Radzen theme is applied and visible**: The page should visually reflect the configured Radzen theme (e.g. Material). Components should have themed colors, rounded inputs, styled buttons, etc. If the page looks like a 1990s unstyled HTML document, something is wrong.
   - **Responsive spot-check**: Resize the browser to a narrow width (~375px) and verify the layout does not overflow or become unusable.
   - **Take a screenshot** of each page after inspecting it. Compare what you see to what the spec describes. If there is a clear mismatch, fix before proceeding.
   - If visual issues are found, fix them before moving on — treat visual defects with the same severity as functional failures.
7. **Do not consider verification complete** until all e2e tests and unit tests pass and the visual quality check is complete

---

### Console Application Verification

If the project is a console application (no web component):

1. Generate integration tests that run the application with various input scenarios
2. Assert on expected output and exit codes
3. Test error handling for invalid inputs
4. Run the fix loop until all tests pass

---

### General Rules

- **Generate tests for every flow** in the Definition of Done — not just the happy path
- **Use realistic test data** in tests (e.g., real-looking names, emails, descriptions — not "test123")
- **Run `dotnet test` after any code changes** to ensure all tests still pass
- **Use Playwright MCP for debugging** — when a test fails and the cause isn't obvious from the test output, start the app and use the Playwright MCP browser to take screenshots, inspect DOM elements, and check the browser console. This is often faster than iterating blindly on the test code.
- **Each e2e test should be implemented as its own subagent call** to avoid context bloat
- **Do not consider verification complete** until:
    - All e2e tests have been generated for every flow in the Definition of Done
    - All tests (unit + e2e) pass
    - The solution builds without errors

---

### File Creation and Editing Best Practices

**Parent Directory Creation**: The SDK `create` tool may fail if parent directories don't exist. **Always create parent directories first** using `powershell` with `New-Item -ItemType Directory -Force` before attempting to create files in those directories.

**Line Ending Issues**: When using the `edit` tool to modify existing files, Windows files use CRLF (`\r\n`) line endings while the `edit` tool may expect LF (`\n`). If string replacement fails with "no match found," normalize your search string to include the actual line endings present in the file, or adjust the replacement logic accordingly.
