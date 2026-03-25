---

# Spec 1: Solution Setup & Core Models

**Status**: ✅ Complete

---

## User Story

**As a developer**, I want a fully scaffolded .NET solution with all projects created, all shared models defined, and all static seed data populated so that every subsequent feature spec can be implemented without revisiting foundational setup.

## Description

This spec covers everything that must exist before any feature can be built: creating the four projects in the solution (SpanishLearning.Core, SpanishLearning.Web, SpanishLearning.Core.Tests, SpanishLearning.Web.Tests), defining all shared model types, wiring up project references, and populating the SeedData static class with enough content to satisfy all features.

The solution is a Blazor WebAssembly app (no backend). SpanishLearning.Core is a class library that holds all models, services, and static data. SpanishLearning.Web is the Blazor WASM front-end. Both test projects use xUnit v3, NSubstitute, and AwesomeAssertions.

## Acceptance Criteria

### Project Scaffolding

- [x] SpanishLearning.Core is created using `create-library-project.ps1`
- [x] SpanishLearning.Web is created using `create-blazor-project.ps1`
- [x] SpanishLearning.Core.Tests is created using `create-test-project.ps1`
- [x] SpanishLearning.Web.Tests is created using `create-test-project.ps1`
- [x] SpanishLearning.Web references SpanishLearning.Core
- [x] SpanishLearning.Core.Tests references SpanishLearning.Core
- [x] SpanishLearning.Web.Tests references SpanishLearning.Web and SpanishLearning.Core
- [x] Solution builds with zero errors and zero warnings

### Models

- [x] `Flashcard` record defined with: `int Id`, `string SpanishWord`, `string EnglishWord`, `string Category`
- [x] `QuizQuestion` record defined with: `int Id`, `string Prompt`, `string[] Options`, `int CorrectIndex`, `string Explanation`
- [x] `Story` record defined with: `int Id`, `string Title`, `string SpanishText`, `string EnglishText`
- [x] `Lesson` record defined with: `int Id`, `string Title`, `LessonSection[] Sections`
- [x] `LessonSection` record defined with: `string Heading`, `string Body`
- [x] `Resource` record defined with: `int Id`, `string Title`, `string Url`, `string Description`
- [x] `ProgressRecord` class defined with: `HashSet<int> LessonsCompleted`, `List<QuizScore> QuizScores`
- [x] `QuizScore` record defined with: `DateTime TakenAt`, `int Score`, `int TotalQuestions`
- [x] All model types are in the `SpanishLearning.Core.Models` namespace

### Seed Data

- [x] `SeedData` static class exists in `SpanishLearning.Core`
- [x] `SeedData.Flashcards` contains ≥ 20 entries across at least 3 categories (greetings, numbers, food)
- [x] `SeedData.QuizQuestions` contains ≥ 10 entries mixing vocabulary and grammar
- [x] `SeedData.Stories` contains exactly 2 stories, each ≥ 3 paragraphs, one beginner and one intermediate
- [x] `SeedData.Lessons` contains exactly 3 lessons: Basic Greetings, Numbers 1–20, Present Tense Verbs
- [x] `SeedData.Resources` contains ≥ 5 curated resource link entries

### Tests

- [x] Unit tests in SpanishLearning.Core.Tests verify that SeedData collections are non-empty and meet minimum count requirements
- [x] All tests pass

## Out of Scope

- Feature services (FlashcardService, QuizService, etc.) — covered in later specs
- Any Blazor pages or UI — covered in later specs
- Progress persistence — covered in the Progress spec

## Technical Notes

- All projects target .NET 10
- Use file-scoped namespaces and record types for models
- SeedData is a `public static class` with `public static readonly` list fields
- Do not use Entity Framework or any database — all data is in-memory static lists
- Test project uses xUnit v3, NSubstitute, AwesomeAssertions

## Files to Create or Modify

- SpanishLearning.Core/Models/Flashcard.cs
- SpanishLearning.Core/Models/QuizQuestion.cs
- SpanishLearning.Core/Models/Story.cs
- SpanishLearning.Core/Models/Lesson.cs
- SpanishLearning.Core/Models/LessonSection.cs
- SpanishLearning.Core/Models/Resource.cs
- SpanishLearning.Core/Models/ProgressRecord.cs
- SpanishLearning.Core/Models/QuizScore.cs
- SpanishLearning.Core/SeedData.cs
- SpanishLearning.Core.Tests/SeedDataTests.cs

## Definition of Done

- [x] All acceptance criteria are met
- [x] Solution scaffolded with all four projects
- [x] All model types compile cleanly
- [x] SeedData is fully populated with realistic Spanish learning content
- [x] Unit tests cover SeedData minimum-count requirements
- [x] All tests pass
- [x] Code reviewed and merged to main branch

---
