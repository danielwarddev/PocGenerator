## MVP Verification Phase (End-to-End User Flow Testing)

You are performing a **final verification pass** on a fully generated MVP project. Your goal is to ensure the application actually works end-to-end by testing all core user flows — not just that it builds and passes unit tests, but that a real user could navigate through the app without encountering errors, broken pages, or incorrect behavior.

This will be a big task, so use subagents for each major flow or feature if needed. The key is to be methodical and thorough — test every feature described in the spec, not just the happy paths. If you find any issues, fix them and re-test until everything works correctly.

---

### Web Application Verification (Blazor, ASP.NET, etc.)

If the project contains a web application component, you **MUST** perform the following:

1. **Start the web application**:
    - Identify the web project in the solution (look for Blazor, ASP.NET, or similar projects)
    - Run it using `dotnet run` from the web project directory
    - Wait for the application to start and note the URL (typically `https://localhost:5001` or `http://localhost:5000`)

2. **Navigate to the application using the Playwright MCP browser**:
    - Open the running application's URL
    - Verify the home/landing page loads without errors

3. **Test ALL core user flows end-to-end**:
    - Read the attached idea/spec files to understand what the application is supposed to do
    - For **each major feature**, perform the complete user workflow:
        - **Navigation**: Click through all menu items, links, and navigation elements. Verify each page loads correctly.
        - **Forms & Data Entry**: Fill in text fields, select dropdowns, check checkboxes, and submit forms. Use realistic test data.
        - **CRUD Operations**: If the app manages data (create, read, update, delete), test all four operations:
            - Create a new item by filling out and submitting the form
            - Verify the created item appears in any list/table views
            - Edit the item and verify changes are saved
            - Delete the item and verify it is removed
        - **Interactive Elements**: Click buttons, open modals/dialogs, expand/collapse sections, test any interactive UI components
        - **Data Display**: Verify tables, charts, cards, and other data displays show information correctly (not blank, not errored)
        - **Error States**: Try submitting forms with empty required fields or invalid data — verify the app handles it gracefully (shows validation messages, not crash pages)

4. **When a flow fails** (error page, exception, blank page, broken layout, incorrect data, unresponsive button, etc.):
    - **Diagnose the root cause**: Read the error message, check the browser console output, examine the relevant source code
    - **Fix the code**: Make targeted changes to resolve the issue
    - **Restart the application** if needed (stop the old process, rebuild, start again)
    - **Re-test the flow** to confirm the fix works
    - **Repeat** until the flow works correctly end-to-end

5. **After fixing issues, re-verify ALL flows**:
    - A fix for one flow may break another
    - Do a complete pass through all flows after any code changes
    - Continue until every core flow passes without errors

6. **Verify visual quality and styling on every page**:
    - **No unstyled elements**: All text, buttons, inputs, cards, and tables must have visible styling — not browser-default unstyled HTML. If elements look like plain, unstyled HTML (e.g. a raw `<input>` box with no padding, a `<button>` with system defaults), the Radzen Blazor components are not being used as required.
    - **All referenced CSS classes must be defined**: Inspect each component's markup. Every CSS class used in a `class="..."` attribute must have a corresponding rule in either the co-located `.razor.css` file or `app.css`. Undefined classes produce unstyled/broken layouts.
    - **Layout coherence**: Content should be centered or padded appropriately, not flush against the viewport edges. Navigation should be clearly separated from page content. Check that headings, spacing, alignment, and visual hierarchy look intentional — not accidental.
    - **No template leftovers**: Default Blazor template pages (Counter.razor, Weather.razor) must NOT exist. NavMenu must only contain app-specific links — not the default "Home / Counter / Weather" entries. The stock `MainLayout.razor.css` sidebar/top-row styles should be replaced with styles matching the actual layout.
    - **Radzen theme is applied and visible**: The page should visually reflect the configured Radzen theme (e.g. Material). Components should have themed colors, rounded inputs, styled buttons, etc. If the page looks like a 1990s unstyled HTML document, something is wrong.
    - **Responsive spot-check**: Resize the browser to a narrow width (~375px) and verify the layout does not overflow or become unusable.
    - **Take a screenshot** of each page after inspecting it. Compare what you see to what the spec describes. If there is a clear mismatch, fix before proceeding.

    If visual issues are found, **fix them before moving on** — treat visual defects with the same severity as functional failures.

7. **Stop the web application** when verification is complete

---

### Console Application Verification

If the project is a console application (no web component):

1. **Run the application** with appropriate command-line arguments
2. **Verify the output** is correct and the program completes without unhandled exceptions
3. **Test different input scenarios** based on the spec requirements
4. If errors occur, fix the code and re-run until all scenarios work

---

### General Rules

- **Do NOT skip flows** — test every feature described in the spec/idea, not just the happy path
- **Use realistic test data** when filling out forms (e.g., real-looking names, emails, descriptions — not "test123")
- **Read error messages carefully** — the root cause is often in the error details or stack trace
- **Run `dotnet test` after any code changes** to ensure unit tests still pass
- **Do not consider verification complete** until:
    - Every core user flow has been tested and works correctly
    - All unit tests pass
    - The application starts and runs without errors
- If the project uses Docker or docker-compose, ensure containers start correctly and the app is accessible

---

### File Creation and Editing Best Practices

**Parent Directory Creation**: The SDK `create` tool may fail if parent directories don't exist. **Always create parent directories first** using `powershell` with `New-Item -ItemType Directory -Force` before attempting to create files in those directories.

**Line Ending Issues**: When using the `edit` tool to modify existing files, Windows files use CRLF (`\r\n`) line endings while the `edit` tool may expect LF (`\n`). If string replacement fails with "no match found," normalize your search string to include the actual line endings present in the file, or adjust the replacement logic accordingly.
