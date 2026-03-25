---

# Spec 4: Stories Feature

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to read short Spanish stories and hold the spacebar to temporarily reveal the English translation so that I can challenge my reading comprehension while having a safety net.

## Description

This spec implements the Stories feature: a service that exposes story data from SeedData, a story-list page at `/stories`, and a story-reader view. The reader displays the Spanish text by default. While the user holds the spacebar, the text switches to the English translation; releasing the key switches back. A visible hint guides the user. JS interop handles keydown/keyup events.

## Acceptance Criteria

### StoryService

- [x] `StoryService` class exists in `SpanishLearning.Core.Services`
- [x] `GetAll()` returns all stories from `SeedData.Stories`
- [x] `GetById(int id)` returns the matching story or throws if not found

### Stories List Page

- [x] Page is routed at `/stories`
- [x] All available stories are listed with their titles
- [x] Clicking a story title navigates to the story reader for that story

### Story Reader

- [x] The reader displays the selected story's Spanish text by default
- [x] A visible on-screen hint reads: "Hold [Space] to reveal English"
- [x] While the spacebar is held down, the displayed text switches to the English translation
- [x] Releasing the spacebar switches the text back to Spanish
- [x] The story title is always visible regardless of the toggle state
- [x] The toggle works via JavaScript `keydown`/`keyup` event listeners wired through Blazor JS interop

### Tests

- [x] Unit tests verify `StoryService.GetAll()` returns the expected count
- [x] Unit tests verify `StoryService.GetById()` returns the correct story
- [x] Unit tests verify `StoryService.GetById()` throws for an unknown id
- [x] All tests pass

## Out of Scope

- Audio narration or text-to-speech
- Word-by-word hover translations
- Progress tracking integration for stories

## Technical Notes

- Register `StoryService` as scoped in `Program.cs` of SpanishLearning.Web
- Create `SpanishLearning.Web/wwwroot/js/storyToggle.js` that exports two functions: `registerKeyListeners(dotNetRef)` and `unregisterKeyListeners()`. On `keydown` of Space it calls `dotNetRef.invokeMethodAsync('OnKeyDown')` and on `keyup` calls `dotNetRef.invokeMethodAsync('OnKeyUp')`
- The Stories reader component holds `bool _showEnglish` and `[JSInvokable]` methods `OnKeyDown` and `OnKeyUp` that toggle it and call `StateHasChanged()`
- Use `IJSRuntime` injected into the component to call `registerKeyListeners` in `OnAfterRenderAsync` (first render only) and `unregisterKeyListeners` in `IAsyncDisposable.DisposeAsync`
- The reader can be the same page component using a query parameter or route parameter: `/stories/{id:int}`
- Tests use xUnit v3 and AwesomeAssertions; no mocking required for StoryService

## Files to Create or Modify

- SpanishLearning.Core/Services/StoryService.cs
- SpanishLearning.Web/Pages/Stories.razor
- SpanishLearning.Web/Pages/StoryReader.razor
- SpanishLearning.Web/wwwroot/js/storyToggle.js
- SpanishLearning.Web/Program.cs (register StoryService)
- SpanishLearning.Core.Tests/StoryServiceTests.cs

## Definition of Done

- [x] All acceptance criteria are met
- [x] StoryService unit tests pass
- [x] Stories list renders and navigation to reader works
- [x] Spacebar key-hold toggles Spanish/English text
- [x] Application builds with zero errors and zero warnings
- [ ] Code reviewed and merged to main branch

---
