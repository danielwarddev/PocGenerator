# Spec 3: Quiz Feature

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to take a multiple-choice quiz on vocabulary and grammar so that I can test my knowledge and see my score at the end.

## Description

This spec implements the Quiz feature: a service that loads quiz questions from SeedData, and a Blazor page at `/quiz` that presents questions one at a time, evaluates answers, and displays a final score. Each question shows a prompt and four multiple-choice options. After selecting an answer the user sees immediate feedback (correct/incorrect with explanation), then advances to the next question. At the end a summary screen shows the total score and a "Try Again" button.

Upon completion, the quiz result is recorded via ProgressService so it appears on the Progress page.

## Acceptance Criteria

### QuizService

- [x] `QuizService` class exists in `SpanishLearning.Core.Services`
- [x] `GetQuestions()` returns all questions from `SeedData.QuizQuestions`
- [x] `GetShuffled()` returns all questions in a randomised order

### Quiz Page

- [x] Page is routed at `/quiz`
- [x] Questions are presented one at a time in sequence
- [x] Each question displays the prompt and exactly four answer options as clickable buttons
- [x] After the user selects an option, the correct answer is highlighted green and any incorrect selection is highlighted red
- [x] The explanation text for the question is displayed after an answer is selected
- [x] A "Next" button appears after an answer is selected and advances to the following question
- [x] After the last question a results screen displays "You scored X / Y"
- [x] A "Try Again" button on the results screen resets the quiz from the beginning
- [x] The completed quiz score is saved via `ProgressService.RecordQuizScore()`
- [x] The current question number and total are shown (e.g. "Question 2 of 10")

### Tests

- [x] Unit tests verify `QuizService.GetQuestions()` returns the expected count
- [x] Unit tests verify `QuizService.GetShuffled()` returns the same items in a (likely) different order
- [x] All tests pass

## Out of Scope

- Timer or time-limited quizzes
- Fill-in-the-blank mode (multiple-choice only for MVP)
- Detailed per-question history on the results screen beyond final score

## Technical Notes

- `QuizService` is a plain class; register as scoped in `Program.cs`
- Quiz state (current question index, selected answer, answers list) lives entirely in the Blazor component as private fields
- Use `System.Random.Shared.Shuffle()` for shuffling in `GetShuffled()`
- ProgressService must be injected into the Quiz page; see the Progress spec for its API
- Tests use xUnit v3 and AwesomeAssertions

## Files to Create or Modify

- SpanishLearning.Core/Services/QuizService.cs
- SpanishLearning.Web/Pages/Quiz.razor
- SpanishLearning.Web/Program.cs (register QuizService)
- SpanishLearning.Core.Tests/QuizServiceTests.cs

## Definition of Done

- [x] All acceptance criteria are met
- [x] QuizService unit tests pass
- [x] Quiz page renders questions, evaluates answers, and shows final score
- [x] Score is persisted via ProgressService
- [x] Application builds with zero errors and zero warnings
- [x] Code reviewed and merged to main branch
