# Spec 1: Replace Manual Verification with E2E Test Generation

**Status**: ✅ Complete

---

## User Story

**As a user**, I want the verification phase to generate and run e2e tests instead of manually browsing the app with Playwright MCP, so that the generated MVP ships with repeatable, automated tests that verify all core user flows.

## Description

The current verification phase uses Playwright MCP to manually navigate and interact with the generated web app in real time — clicking through pages, filling forms, and visually checking results. This is slow, flaky, and leaves no reusable artifact behind.

This spec replaces that approach: instead of manually testing user flows, the verification phase instructs Copilot to **generate e2e test projects** that cover all user flows defined in the implementation plan's Definition of Done. The generated tests become part of the MVP output — anyone can re-run them later with `dotnet test`.

The verification phase should generate both **Playwright browser tests** (for web UI flows) and **HTTP integration tests** (for API-level flows), with guidance on when to use each. Each e2e test should be implemented as its own subagent call to avoid context bloat. After generation, the fix loop runs `dotnet test` and iterates until all tests pass (or attempts are exhausted), matching the existing fix-loop behavior for builds.

README generation at the end of verification is unchanged.

## Acceptance Criteria

### Verification Prompt Changes

- [x] `VerificationPromptTemplate` instructs Copilot to read the implementation plan's Definition of Done to identify all user flows that need e2e coverage
- [x] The prompt instructs Copilot to create a Playwright e2e test project for web UI flows (page navigation, form submission, CRUD operations, interactive elements)
- [x] The prompt instructs Copilot to create HTTP integration tests (using `WebApplicationFactory`) for API-level flows that don't require a browser
- [x] The prompt provides guidance on when to use Playwright vs. HTTP integration tests (e.g., use Playwright when the flow involves browser rendering, JS interop, or visual layout; use HTTP tests for pure API request/response verification)
- [x] The prompt instructs Copilot to implement each e2e test as a separate subagent to avoid context bloat
- [x] The prompt includes a fix loop: run `dotnet test`, diagnose failures, fix code, re-test until green
- [x] The prompt attaches the implementation plan file (not just the idea file) so Copilot has access to the Definition of Done

### System Prompt Changes

- [x] `SystemPrompt_Verification.md` replaces all manual Playwright MCP browsing instructions with e2e test generation guidance
- [x] The system prompt describes Playwright .NET test patterns (launching the app, navigating pages, asserting DOM elements)
- [x] The system prompt describes HTTP integration test patterns (`WebApplicationFactory`, `HttpClient`, asserting JSON responses)
- [x] The system prompt removes references to "manually navigate," "click through," "visually verify," and similar manual-testing language

### VerificationPhaseHandler Code Changes

- [x] `VerifyMvp` attaches the `implementation-plan.md` file path (in addition to or instead of the idea file) when sending the verification prompt
- [x] The implementation plan file path is derived from the output directory (known location: `{outputDirectory}/implementation-plan.md`)

### README Generation

- [x] README generation in the second session remains unchanged
- [x] README session still receives the idea file as an attachment

### Backwards Compatibility

- [x] No changes to `ConsoleRunner`, `IVerificationPhaseHandler` interface signature, or DI registrations
- [x] No changes to the planning or generation phases
- [x] Existing unit tests in `VerificationPhaseHandlerTests.cs` are updated to reflect the new prompt content and attached file paths

## Out of Scope

- Changes to the planning phase or spec splitting — the implementation plan's Definition of Done is the flow source, no new planning artifacts needed
- Changes to the generation phase or `CodeGenerator`
- Adding new phase handlers or new interfaces
- Generating e2e tests during the generation phase (they are generated during verification)
- Running the app and manually verifying it still works (the e2e tests do this automatically now)

## Technical Notes

- The implementation plan file is already written to `{outputDirectory}/implementation-plan.md` by `ProjectPlanner` during the planning phase — no new file creation needed
- The `VerificationPhaseHandler` already has access to `outputDirectory`, so constructing the plan file path is straightforward
- The `IIdeaFileLocator` dependency may still be needed for the README session, but the verification session should primarily use the implementation plan
- The `create_test_project` scaffolding tool is already available during Copilot sessions with the developing system prompt; the verification system prompt should reference it or ensure it's available
- Playwright .NET (`Microsoft.Playwright`) would need to be added as a package in the generated e2e test project — the verification prompt should instruct Copilot to do this
- Subagent usage for each test is a prompt-level instruction — no code changes needed to support it, Copilot SDK handles subagent delegation

## Definition of Done

- [x] All acceptance criteria are met
- [x] `SystemPrompt_Verification.md` contains no references to manual browser testing
- [x] `VerificationPromptTemplate` references the implementation plan's Definition of Done as the source for e2e test scenarios
- [x] `VerificationPhaseHandler` passes the implementation plan file path to the Copilot session
- [x] `dotnet build PocGenerator` succeeds with no errors
- [x] All existing tests in `VerificationPhaseHandlerTests.cs` are updated and pass with `dotnet test PocGenerator.Tests`
