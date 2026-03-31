# Spec 4: Retry from Failure

**Status**: ✅ Complete

---

## User Story

**As a user**, I want to pass `--retry <path>` to resume a failed run from the last successful checkpoint so that I don't have to re-run the entire pipeline from scratch.

## Description

When the pipeline fails mid-run, the git commits from SPEC-03 serve as checkpoints. This spec adds a single `--retry` CLI option that takes the previous output directory path, auto-detects the last successful phase from the git log, discards uncommitted work, and resumes from the next phase.

## Acceptance Criteria

### CLI Flags

- [x] A `--retry` string option is added to `CliArgs`; when omitted, the app runs the normal non-resume flow
- [x] The `--retry` value specifies the output directory of a previous failed run
- [x] When `--retry` is provided with a missing or empty value, the app exits with a clear error message
- [x] The retry flow does not perform upfront validation that the provided directory exists or contains a `.git/` folder

### Phase Detection

- [x] On `--retry`, the app reads the latest git commit message from the provided retry directory to determine the last successful phase
- [x] Uncommitted changes from the failed phase are discarded using `git clean -fd` and `git checkout .`
- [x] If there are no commits, the app exits with a clear message telling the user to start a new run
- [x] If the last commit is `planning`, resume from generation (spec 1)
- [x] If the last commit is `spec <N>`, resume from spec N+1 (or verification if all specs are done)
- [x] If the last commit is `verification`, the app logs that the run already completed successfully and exits

### Project Plan Reconstruction

- [x] The existing `ProjectPlan` is reconstructed from the output directory's files rather than re-running planning
- [x] The plan file is read from `implementation-plan.md` in the output directory
- [x] Spec files are read from the `Specs/` subdirectory in the output directory
- [x] The provided `--retry` directory is reused as the output directory for the remaining phases

### ConsoleRunner Integration

- [x] `ConsoleRunner` branches on whether `--retry` has a value: normal flow vs. resume flow
- [x] `CopilotService.Initialize` is still called on resume (the Copilot client is needed for remaining phases)
- [x] All existing tests continue to pass

## Out of Scope

- Resuming from a partial spec (only full spec boundaries are checkpoints)
- Automatic retry without user intervention (the user must explicitly pass `--retry`)
- Remote git operations

## Technical Notes

- Uses `GitService.GetLog` and `GitService.CleanAndRestore` from SPEC-02
- Commit messages follow the exact convention from SPEC-03: `planning`, `spec 1` … `spec N`, `verification`
- Phase detection matches the most recent commit message exactly; only the most recent commit matters
- A new `IProjectPlanReconstructor` / `ProjectPlanReconstructor` service handles rebuilding the `ProjectPlan` from disk

## Definition of Done

- [x] All acceptance criteria are met
- [x] Unit tests cover `--retry` CLI argument validation (missing value, empty value)
- [x] Unit tests cover phase detection from git log (all commit message patterns)
- [x] Unit tests cover `ProjectPlanReconstructor` (plan file and spec file reconstruction)
- [x] Existing tests pass (`dotnet test PocGenerator.Tests`)
- [x] Build succeeds (`dotnet build PocGenerator`)
