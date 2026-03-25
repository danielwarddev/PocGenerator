# Spec 8: Dashboard & Navigation

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want a welcoming home page with clear navigation to every feature so that I can quickly find what I want to study next.

## Description

This spec implements the application shell: a top-level navigation layout with links to all six feature areas, and a Dashboard home page at `/` that shows nav cards for each feature with a brief description. The Dashboard also surfaces a quick progress summary (lessons completed, last quiz score) pulled from ProgressService. This is the final spec since it ties all features together visually.

## Acceptance Criteria

### Navigation Layout

- [x] A persistent navigation bar or sidebar is present on every page
- [x] Navigation contains links to: Home (`/`), Flashcards (`/flashcards`), Quiz (`/quiz`), Stories (`/stories`), Lessons (`/lessons`), Progress (`/progress`), Resources (`/resources`)
- [x] The currently active route is visually highlighted in the navigation

### Dashboard Page

- [x] Page is routed at `/`
- [x] Page displays a heading such as "Welcome to Spanish Learning"
- [x] Six feature cards are displayed, one per feature area (Flashcards, Quiz, Stories, Lessons, Progress, Resources), each with a title, short description, and a link/button to navigate to that feature
- [x] A quick-stats section shows: number of lessons completed and the most recent quiz score (or "No attempts yet" if none)
- [x] Quick stats are loaded from ProgressService on page initialisation

### Tests

- [x] No unit tests are required for the Dashboard page itself (pure presentation, no logic)

## Out of Scope

- User accounts or personalised greetings
- Notifications or reminders
- Any analytics or telemetry

## Technical Notes

- The navigation layout lives in `SpanishLearning.Web/Shared/NavMenu.razor` (or `MainLayout.razor`)
- Use Blazor's `<NavLink>` component with `ActiveClass` for active-route highlighting
- The Dashboard injects `ProgressService` to fetch quick stats
- No new services are introduced in this spec; it only consumes existing ones

## Files to Create or Modify

- SpanishLearning.Web/Pages/Index.razor (Dashboard page)
- SpanishLearning.Web/Shared/MainLayout.razor (add nav links)
- SpanishLearning.Web/Shared/NavMenu.razor (navigation component)

## Definition of Done

- [x] All acceptance criteria are met
- [x] Dashboard renders feature cards and quick stats
- [x] Navigation is present on every page with active-link highlighting
- [x] Application builds with zero errors and zero warnings
- [x] Code reviewed and merged to main branch
