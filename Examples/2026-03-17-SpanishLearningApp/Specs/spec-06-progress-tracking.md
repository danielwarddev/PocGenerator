# Spec 6: Progress Tracking

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to see a summary of my completed lessons and quiz scores so that I can track how far I have come and stay motivated.

## Description

This spec implements ProgressService, which persists a ProgressRecord to browser localStorage via Blazor JS interop, and the Progress page at `/progress` that visualises the stored data. Other features (Quiz, Lessons) depend on ProgressService to record activity, so this spec should be completed before or alongside those specs.

## Acceptance Criteria

### ProgressService

- [x] `ProgressService` class exists in `SpanishLearning.Core.Services`
- [x] Constructor accepts `IJSRuntime`
- [x] `GetProgress()` deserialises the JSON stored under key `"spanish-learning-progress"` in localStorage; returns a new empty `ProgressRecord` if nothing is stored
- [x] `RecordLessonComplete(int id)` adds the lesson id to `ProgressRecord.LessonsCompleted` and saves back to localStorage
- [x] `RecordQuizScore(QuizScore score)` appends the score to `ProgressRecord.QuizScores` and saves back to localStorage
- [x] Data persists across page reloads (survives browser refresh)

### Progress Page

- [x] Page is routed at `/progress`
- [x] Displays total number of lessons completed (e.g. "Lessons Completed: 2 / 3")
- [x] Lists each completed lesson by id or name
- [x] Displays a history of quiz attempts showing date taken, score, and total questions for each attempt
- [x] If no progress has been recorded, displays a friendly "No progress yet — start learning!" message
- [x] Page loads data from ProgressService on initialisation

### Tests

- [x] Unit tests for `ProgressService` mock `IJSRuntime` and verify `GetProgress()` returns an empty record when localStorage returns null
- [x] Unit tests verify `RecordLessonComplete()` adds the id to the completed set
- [x] Unit tests verify `RecordQuizScore()` appends the score to the list
- [x] All tests pass

## Out of Scope

- Charts or graphs (simple text/stat cards are sufficient for MVP)
- Resetting or clearing progress from the UI
- Flashcard review tracking

## Technical Notes

- `ProgressService` uses `IJSRuntime.InvokeAsync<string>("localStorage.getItem", key)` and `IJSRuntime.InvokeVoidAsync("localStorage.setItem", key, json)` for persistence
- Use `System.Text.Json.JsonSerializer` for serialisation; no third-party JSON libraries
- Register `ProgressService` as scoped in `Program.cs` of SpanishLearning.Web
- For tests, mock `IJSRuntime` with NSubstitute; configure it to return `null` or a JSON string as needed
- Tests use xUnit v3, NSubstitute, and AwesomeAssertions

## Files to Create or Modify

- SpanishLearning.Core/Services/ProgressService.cs
- SpanishLearning.Web/Pages/Progress.razor
- SpanishLearning.Web/Program.cs (register ProgressService)
- SpanishLearning.Core.Tests/ProgressServiceTests.cs

## Definition of Done

- [x] All acceptance criteria are met
- [x] ProgressService unit tests pass with mocked IJSRuntime
- [x] Progress page renders completed lessons and quiz history
- [x] Data persists across page reloads
- [x] Application builds with zero errors and zero warnings
- [x] Code reviewed and merged to main branch
