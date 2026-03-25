## Project Goal

A browser-based Spanish learning web application for English speakers who want to study Spanish at their own pace. The MVP provides flashcards, quizzes, short bilingual stories, structured lessons, a progress tracker, and curated resource links — all backed by static seed data with no external API calls.

## Requirements

- Display a set of Spanish vocabulary flashcards that the user can flip to reveal the English translation
- Present quizzes (multiple-choice or fill-in-the-blank) based on vocabulary and lesson content
- Show short stories written in Spanish; while the user holds a designated key, the story switches to its English translation, then reverts when released
- Provide structured lessons covering grammar topics, vocabulary sets, or conversational phrases
- Track per-user progress (lessons completed, quiz scores, flashcards reviewed) stored locally in the browser via `localStorage`
- Display a curated list of links to helpful external Spanish learning resources (links are static data; no HTTP calls are made at runtime)
- All content (flashcards, quiz questions, stories, lessons, resource links) is seeded as static C# data — no database, no HTTP API calls

## Definition of Done

- [ ] Flashcard deck is viewable; cards flip on interaction to reveal translation
- [ ] At least one quiz mode works end-to-end: questions load, answers are evaluated, and a score is displayed
- [ ] At least two short stories are available; holding the designated key (e.g. Space) live-swaps the visible text to English and releases back to Spanish
- [ ] At least three lessons are available and fully readable
- [ ] Progress tracker records and displays quiz scores and lesson completion across page reloads (via `localStorage`)
- [ ] Resource links page renders all seeded links as clickable anchors
- [ ] All seeded content is plentiful enough to demonstrate the feature (≥ 20 flashcards, ≥ 10 quiz questions, ≥ 2 stories, ≥ 3 lessons)
- [ ] Application builds with zero errors and zero warnings
- [ ] All unit tests pass

## Architecture Overview

The app is a **Blazor WebAssembly** single-page application. All content lives in a static data layer (plain C# classes) inside a shared class library (`SpanishLearning.Core`). The Blazor app references this library and renders each feature as a separate Blazor page/component.

**Key design decisions:**
- **Vertical slicing**: each feature (Flashcards, Quiz, Stories, Lessons, Progress, Resources) is a self-contained Blazor page with its own component(s) and service
- **No backend**: pure client-side WASM — nothing phones home
- **Static seed data**: a `SeedData` static class in `Core` returns pre-populated lists; the Blazor app never calls `HttpClient` for data
- **Progress persistence**: a `ProgressService` reads/writes JSON to `localStorage` via JS interop — no server, no database
- **Key-hold story toggle**: JavaScript `keydown`/`keyup` event listeners wired via Blazor JS interop toggle a boolean in the Story component

Component interaction flow:
```
SpanishLearning.Web (Blazor WASM)
  └── references SpanishLearning.Core
        ├── SeedData (static content)
        ├── Feature services (FlashcardService, QuizService, StoryService, LessonService, ProgressService, ResourceService)
        └── Models (Flashcard, QuizQuestion, Story, Lesson, ProgressRecord, Resource)
```

## Solution Structure

| Project Name | Script | Purpose |
|---|---|---|
| SpanishLearning.Core | `create-library-project.ps1` | Models, seed data, and feature services shared across the solution |
| SpanishLearning.Web | `create-blazor-project.ps1` | Blazor WebAssembly front-end; all pages and UI components |
| SpanishLearning.Core.Tests | `create-test-project.ps1` | Unit tests for Core services and seed data |
| SpanishLearning.Web.Tests | `create-test-project.ps1` | bUnit component tests for Blazor pages and components |

## Project References

- `SpanishLearning.Web` → `SpanishLearning.Core`
- `SpanishLearning.Core.Tests` → `SpanishLearning.Core`
- `SpanishLearning.Web.Tests` → `SpanishLearning.Web`, `SpanishLearning.Core`

## Implementation Notes

### Libraries
- **Blazor WebAssembly** (.NET 10) for the front-end
- **bUnit** for Blazor component tests
- **xUnit v3** + **NSubstitute** + **AwesomeAssertions** for all test projects
- No Entity Framework, no HTTP client, no third-party UI libraries required

### Data Modeling (`SpanishLearning.Core`)
```
Flashcard       { Id, SpanishWord, EnglishWord, Category }
QuizQuestion    { Id, Prompt, Options (string[]), CorrectIndex, Explanation }
Story           { Id, Title, SpanishText, EnglishText }
Lesson          { Id, Title, Sections (LessonSection[]) }
LessonSection   { Heading, Body }
Resource        { Id, Title, Url, Description }
ProgressRecord  { LessonsCompleted (HashSet<int>), QuizScores (List<QuizScore>) }
QuizScore       { TakenAt, Score, TotalQuestions }
```

### Services (`SpanishLearning.Core`)
Each service is a plain class (no interface required for MVP) that receives data from `SeedData` and exposes simple query methods. `ProgressService` uses constructor-injected `IJSRuntime` and calls `localStorage.getItem`/`setItem` via JS interop.

### Seed Data
`SeedData` is a static class with static readonly lists. Provide:
- ≥ 20 flashcards across at least 3 categories (greetings, numbers, food)
- ≥ 10 quiz questions mixing vocabulary and simple grammar
- 2 short stories (≥ 3 paragraphs each), one beginner, one intermediate
- 3 lessons: (1) Basic Greetings, (2) Numbers 1–20, (3) Present Tense Verbs

### Story Key-Hold Toggle
In the Story Blazor component, register a JavaScript module (`storyToggle.js`) that fires a .NET callback on `keydown`/`keyup` for the spacebar. The component flips a `bool _showEnglish` field and calls `StateHasChanged()`. Include a visible on-screen hint: *"Hold [Space] to reveal English."*

### Routing
Use Blazor's built-in `@page` directive for each feature:
- `/` — Dashboard / home with nav cards
- `/flashcards` — Flashcard deck
- `/quiz` — Quiz mode
- `/stories` — Story list and reader
- `/lessons` — Lesson list and reader
- `/progress` — Progress tracker
- `/resources` — Resource link list

### Progress Tracking
`ProgressService` serializes/deserializes `ProgressRecord` as JSON in `localStorage` under the key `"spanish-learning-progress"`. It exposes `RecordLessonComplete(int id)`, `RecordQuizScore(QuizScore score)`, and `GetProgress()`. The Progress page reads from this service and renders charts or simple stat cards.