---

# Spec 5: Lessons Feature

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to read structured lessons on grammar and vocabulary so that I can learn Spanish concepts in an organised, step-by-step format.

## Description

This spec implements the Lessons feature: a service that surfaces lesson data from SeedData, a lessons-list page at `/lessons`, and a lesson-reader page at `/lessons/{id:int}`. Each lesson is divided into named sections (heading + body text). When the user finishes reading a lesson they can mark it complete, which is persisted via ProgressService.

The three seed lessons are: (1) Basic Greetings, (2) Numbers 1–20, (3) Present Tense Verbs.

## Acceptance Criteria

### LessonService

- [ ] `LessonService` class exists in `SpanishLearning.Core.Services`
- [ ] `GetAll()` returns all lessons from `SeedData.Lessons`
- [ ] `GetById(int id)` returns the matching lesson or throws if not found

### Lessons List Page

- [ ] Page is routed at `/lessons`
- [ ] All available lessons are listed with their titles
- [ ] Lessons already marked complete (from ProgressService) display a visual indicator (e.g. a checkmark)
- [ ] Clicking a lesson title navigates to the lesson reader for that lesson

### Lesson Reader Page

- [ ] Page is routed at `/lessons/{id:int}`
- [ ] The lesson title is displayed as a heading
- [ ] Each section is rendered with its heading and body text
- [ ] A "Mark as Complete" button is displayed at the bottom of the lesson
- [ ] Clicking "Mark as Complete" calls `ProgressService.RecordLessonComplete(id)` and updates the button to show "✓ Completed"
- [ ] If the lesson is already complete when the page loads, the button shows "✓ Completed" immediately

### Tests

- [ ] Unit tests verify `LessonService.GetAll()` returns the expected count (3)
- [ ] Unit tests verify `LessonService.GetById()` returns the correct lesson
- [ ] Unit tests verify `LessonService.GetById()` throws for an unknown id
- [ ] All tests pass

## Out of Scope

- Inline exercises or quizzes embedded within lessons
- Lesson ordering or prerequisites
- Audio or video content

## Technical Notes

- Register `LessonService` as scoped in `Program.cs` of SpanishLearning.Web
- The lesson reader injects both `LessonService` and `ProgressService`
- Use `ProgressService.GetProgress()` on init to check if the lesson is already complete
- Tests use xUnit v3 and AwesomeAssertions; no mocking required for LessonService

## Files to Create or Modify

- SpanishLearning.Core/Services/LessonService.cs
- SpanishLearning.Web/Pages/Lessons.razor
- SpanishLearning.Web/Pages/LessonReader.razor
- SpanishLearning.Web/Program.cs (register LessonService)
- SpanishLearning.Core.Tests/LessonServiceTests.cs

## Definition of Done

- [ ] All acceptance criteria are met
- [ ] LessonService unit tests pass
- [ ] Lessons list renders with completion indicators
- [ ] Lesson reader renders all sections and mark-complete persists
- [ ] Application builds with zero errors and zero warnings
- [ ] Code reviewed and merged to main branch

---
