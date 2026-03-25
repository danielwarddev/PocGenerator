# Spec 7: Resources Page

**Status**: ✅ Complete

---

## User Story

**As a Spanish learner**, I want to see a curated list of helpful external learning resources so that I can supplement my in-app study with other tools and websites.

## Description

This spec implements the Resources feature: a lightweight service that exposes the static resource list from SeedData, and a Blazor page at `/resources` that renders each resource as a clickable link with a title and short description. All resource data is static — no HTTP calls are made at runtime.

## Acceptance Criteria

### ResourceService

- [x] `ResourceService` class exists in `SpanishLearning.Core.Services`
- [x] `GetAll()` returns all resources from `SeedData.Resources`

### Resources Page

- [x] Page is routed at `/resources`
- [x] All resources are listed, each showing: title as a clickable hyperlink, and description below it
- [x] Links open in a new browser tab (`target="_blank"`)
- [x] At least 5 resources are displayed (sourced from SeedData)

### Tests

- [x] Unit tests verify `ResourceService.GetAll()` returns the expected count (≥ 5)
- [x] All tests pass

## Out of Scope

- User-submitted or user-saved resources
- Resource categories or filtering
- Any HTTP calls to verify resource URLs

## Technical Notes

- Register `ResourceService` as scoped in `Program.cs` of SpanishLearning.Web
- The page uses a simple `@foreach` loop over `ResourceService.GetAll()`
- Use `<a href="@resource.Url" target="_blank" rel="noopener noreferrer">` for each link
- Tests use xUnit v3 and AwesomeAssertions; no mocking required

## Files to Create or Modify

- SpanishLearning.Core/Services/ResourceService.cs
- SpanishLearning.Web/Pages/Resources.razor
- SpanishLearning.Web/Program.cs (register ResourceService)
- SpanishLearning.Core.Tests/ResourceServiceTests.cs

## Definition of Done

- [x] All acceptance criteria are met
- [x] ResourceService unit tests pass
- [x] Resources page renders all seeded links with titles and descriptions
- [x] Links open in a new tab
- [x] Application builds with zero errors and zero warnings
- [ ] Code reviewed and merged to main branch
