# Spec 3: Phase Commits

**Status**: ✅ Complete

---

## User Story

**As a user**, I want the app to commit after each completed phase so that I have checkpoints I can resume from if a later phase fails.

## Description

Building on the git repository established in SPEC-02, this spec adds automatic commits at each pipeline checkpoint. Commit messages follow a parseable convention so the resume logic (SPEC-04) can determine the last successful phase from the git log.

## Acceptance Criteria

### Commit Checkpoints

- [x] After planning completes (plan + spec splitting), all changes are committed with message `planning`
- [x] After each spec's code generation completes, all changes are committed with message `spec <N>` (e.g., `spec 1`, `spec 2`)
- [x] After verification completes, all changes are committed with message `verification`

### Commit Convention

- [x] Each commit uses `git add -A` followed by `git commit -m "<message>"`
- [x] Commit messages are exact strings: `planning`, `spec 1` … `spec N`, `verification` (the resume logic in SPEC-04 will match these exactly from `git log --oneline`)

### Integration

- [x] `PlanningPhaseHandler` commits `planning` after spec splitting
- [x] `GenerationPhaseHandler` (or `CodeGenerator`) commits after each spec is generated
- [x] `VerificationPhaseHandler` commits after verification is done
- [x] All existing tests continue to pass

## Out of Scope

- Resume/retry logic (SPEC-04)
- Git init and `.gitignore` setup (SPEC-02)

## Technical Notes

- Uses `GitService.AddAll` and `GitService.Commit` from SPEC-02
- The spec number `<N>` in commit messages is the 1-based index of the spec in the generation order (e.g., `spec 1`, `spec 2`)

## Definition of Done

- [x] All acceptance criteria are met
- [x] Unit tests verify commit calls happen at the correct points in each phase handler
- [x] Existing tests pass (`dotnet test PocGenerator.Tests`)
- [x] Build succeeds (`dotnet build PocGenerator`)
