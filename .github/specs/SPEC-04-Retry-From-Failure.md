# Spec 4: Retry from Failure

**Status**: 📋 Not Started

---

## User Story

**As a user**, I want to pass `--retry --resume-dir <path>` to resume a failed run from the last successful checkpoint so that I don't have to re-run the entire pipeline from scratch.

## Description

When the pipeline fails mid-run, the git commits from SPEC-03 serve as checkpoints. This spec adds `--retry` and `--resume-dir` CLI flags that let the user point the app at a previous output directory, auto-detect the last successful phase from the git log, discard uncommitted work, and resume from the next phase.

## Acceptance Criteria

### CLI Flags

- [ ] A `--retry` boolean flag is added to `CliArgs` (default `false`)
- [ ] A `--resume-dir` string flag is added to `CliArgs` to specify the output directory of a previous failed run
- [ ] When `--retry` is provided without `--resume-dir`, the app exits with a clear error message
- [ ] When `--resume-dir` is provided without `--retry`, the app exits with a clear error message
- [ ] When `--resume-dir` points to a directory that doesn't exist or has no `.git/` folder, the app exits with a clear error message

### Phase Detection

- [ ] On `--retry`, the app reads `git log --oneline` from the `--resume-dir` to determine the last successful phase
- [ ] Uncommitted changes from the failed phase are discarded using `git clean -fd` and `git checkout .`
- [ ] If there are no commits, resume from planning
- [ ] If the last commit is `planning`, resume from generation (spec 1)
- [ ] If the last commit is `spec <N>`, resume from spec N+1 (or verification if all specs are done)
- [ ] If the last commit is `verification`, the app logs that the run already completed successfully and exits

### Project Plan Reconstruction

- [ ] The existing `ProjectPlan` is reconstructed from the output directory's files rather than re-running planning
- [ ] The plan file is read from `implementation-plan.md` in the output directory
- [ ] Spec files are read from the `specs/` subdirectory in the output directory
- [ ] The slug is inferred from the output directory folder name (the part after the date prefix)
- [ ] The original `--output` base path is inferred from the `--resume-dir` parent directory

### ConsoleRunner Integration

- [ ] `ConsoleRunner` branches on whether `--retry` is set: normal flow vs. resume flow
- [ ] `CopilotService.Initialize` is still called on resume (the Copilot client is needed for remaining phases)
- [ ] All existing tests continue to pass

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

- [ ] All acceptance criteria are met
- [ ] Unit tests cover CLI argument validation (`--retry` / `--resume-dir` combinations)
- [ ] Unit tests cover phase detection from git log (all commit message patterns)
- [ ] Unit tests cover `ProjectPlanReconstructor` (plan file, spec files, slug extraction)
- [ ] Existing tests pass (`dotnet test PocGenerator.Tests`)
- [ ] Build succeeds (`dotnet build PocGenerator`)
