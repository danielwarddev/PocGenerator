# Spec 2: Flashcards Feature

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to browse a deck of flashcards and flip each card to reveal its English translation so that I can memorize vocabulary at my own pace.

## Description

This spec implements the end-to-end Flashcards feature: a service that surfaces flashcard data from SeedData, and a Blazor page at `/flashcards` that renders the deck. Each card displays a Spanish word on the front; clicking (or tapping) flips it to show the English translation and category. The user can navigate through the deck with Previous/Next buttons or reset to the beginning.

## Acceptance Criteria

### FlashcardService

- [ ] `FlashcardService` class exists in `SpanishLearning.Core.Services`
- [ ] `GetAll()` returns all flashcards from `SeedData.Flashcards`
- [ ] `GetByCategory(string category)` returns only cards matching the given category (case-insensitive)
- [ ] `GetCategories()` returns the distinct list of category names

### Flashcards Page

- [ ] Page is routed at `/flashcards`
- [ ] Page displays the current card index (e.g. "Card 3 of 20")
- [ ] Each card shows the Spanish word on the front face
- [ ] Clicking/tapping the card flips it to reveal the English translation and category
- [ ] Clicking/tapping a flipped card flips it back to Spanish
- [ ] Previous and Next buttons navigate between cards; Previous is disabled on the first card and Next is disabled on the last
- [ ] A category filter dropdown allows the user to view only cards in a chosen category (including an "All" option)
- [ ] Navigating to a new card always resets the flip state back to Spanish side

### Tests

- [ ] Unit tests for `FlashcardService.GetAll()` verify the returned count matches SeedData
- [ ] Unit tests for `FlashcardService.GetByCategory()` verify filtering works correctly
- [ ] Unit tests for `FlashcardService.GetCategories()` verify distinct categories are returned
- [ ] All tests pass

## Out of Scope

- Spaced-repetition or scoring on flashcards
- Animations for the flip effect beyond a simple toggle
- Progress tracking integration for flashcards (covered in the Progress spec)

## Technical Notes

- `FlashcardService` is a plain class (no interface needed for MVP); register it as a scoped service in `Program.cs` of SpanishLearning.Web
- The flip state is a `bool _isFlipped` field on the Blazor component
- Category filter uses a `<select>` element bound to a component field; filtering re-loads the displayed list and resets the current index to 0
- Use Blazor's `@onclick` directive for the flip interaction
- Tests use xUnit v3 and AwesomeAssertions; no mocking required since FlashcardService has no external dependencies

## Files to Create or Modify

- SpanishLearning.Core/Services/FlashcardService.cs
- SpanishLearning.Web/Pages/Flashcards.razor
- SpanishLearning.Web/Program.cs (register FlashcardService)
- SpanishLearning.Core.Tests/FlashcardServiceTests.cs

## Definition of Done

- [ ] All acceptance criteria are met
- [ ] FlashcardService unit tests pass
- [ ] Flashcards page renders and flip interaction works
- [ ] Category filter narrows the visible deck
- [ ] Application builds with zero errors and zero warnings
- [ ] Code reviewed and merged to main branch
