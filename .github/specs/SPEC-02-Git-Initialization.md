# Spec 2: Git Initialization & .gitignore

**Status**: 📋 Not Started

---

## User Story

**As a user**, I want the generated project to be a git repository with a proper `.gitignore` so that it is version-controlled from the start and build artifacts are excluded.

## Description

Before the pipeline can create checkpoint commits, the output directory needs to be a git repository. This spec adds a `.gitignore` file to `ProjectScripts/` (so it ships with the app), copies it into each new project during creation, and initializes a git repo with a `main` branch. A new `IGitService` / `GitService` encapsulates all git operations using the existing `IProcessRunner`.

## Acceptance Criteria

### .gitignore

- [ ] A `.gitignore` file exists in `ProjectScripts/` covering standard .NET ignores (`bin/`, `obj/`, `*.user`, `.vs/`, etc.)
- [ ] `OutputDirectoryService` copies the `.gitignore` into the output directory during project creation (alongside existing script/props copies)

### Git Service

- [ ] A new `IGitService` / `GitService` wraps git operations using `IProcessRunner`
- [ ] `GitService` exposes an `Init` method that runs `git init` and `git branch -M main` in the output directory
- [ ] `GitService` exposes `AddAll` and `Commit(message)` methods for use by later specs
- [ ] `GitService` exposes a `GetLog` method that returns the git log (for use by later specs)
- [ ] `GitService` exposes a `CleanAndRestore` method that runs `git clean -fd` and `git checkout .` (for use by later specs)

### Integration

- [ ] `PlanningPhaseHandler` calls `GitService.Init` after the output directory is created (before planning begins)
- [ ] All existing tests continue to pass

## Out of Scope

- Committing after phases (SPEC-03)
- Resume/retry logic (SPEC-04)
- Remote git operations (push, pull, remote add)

## Technical Notes

- `IProcessRunner` already supports running shell commands; `GitService` should use it for all git operations
- `OutputDirectoryService.CopyGitignore` follows the same pattern as existing `CopyProjectScripts` and `CopyDirectoryBuildProps`
- Git init should happen after all files are copied so the initial state is clean

## Definition of Done

- [ ] All acceptance criteria are met
- [ ] Unit tests cover `GitService` operations (init, add, commit, log, clean)
- [ ] Unit tests cover `.gitignore` copy in `OutputDirectoryService`
- [ ] Existing tests pass (`dotnet test PocGenerator.Tests`)
- [ ] Build succeeds (`dotnet build PocGenerator`)
