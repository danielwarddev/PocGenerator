---
name: spec-creation
description: Guidelines for creating feature specification documents for the Budgeting application. Use this skill when the user asks you to create a new spec or flesh out an existing one, write a user story, or document feature requirements.
---

# Spec Creation

## Overview

This skill defines how to create feature specification documents for the app. Specs are user-story-driven documents with clear acceptance criteria that tell you when you're done.

Before writing a spec, use the `#askQuestions` tool to ask any clarifying questions necessary to fully understand the feature requirements, scope, and constraints.

## File Naming & Location

- **Location**: `.github/specs/`
- **Naming**: `SPEC-XX-Feature-Name.md` (e.g., `SPEC-01-Transaction-Import.md`)
- **Numbering**: Use sequential two-digit numbers (01, 02, 03...)

## Spec Structure

Every spec document MUST include these sections in order:

### 1. Title & Status

```markdown
# Spec X: Feature Name

**Status**: 📋 Not Started | 🚧 In Progress | ✅ Complete

---
```

### 2. User Story

Write from the user's perspective using this format:

```markdown
## User Story

**As a user**, I want to [goal/desire] so that [benefit/reason].
```

### 3. Description

A paragraph or two explaining the feature in more detail. Provide context about why this feature matters and how it fits into the larger application.

### 4. Acceptance Criteria

This is the most important section. Organize criteria into logical groups with checkboxes:

```markdown
## Acceptance Criteria

### [Feature Area 1]

- [ ] Specific, testable requirement
- [ ] Another specific requirement
- [ ] Requirements should be binary (done or not done)

### [Feature Area 2]

- [ ] More requirements...
```

**Guidelines for acceptance criteria:**

- Each item must be specific and testable
- Use action verbs (User can..., System detects..., Page displays...)
- Avoid vague terms like "should work well" or "is user-friendly"
- Group related criteria under descriptive headings
- Every criterion becomes a checklist item during implementation

### 5. Out of Scope

Explicitly state what is NOT included in this spec to prevent scope creep:

```markdown
## Out of Scope

- Feature X (will be handled in Spec Y)
- Edge case Z (deferred to future iteration)
```

### 6. Technical Notes

Developer-focused context that helps with implementation:

```markdown
## Technical Notes

- Relevant existing code/projects
- Database considerations
- Integration points
- Performance considerations
```

### 7. UI Wireframe Concepts (Optional)

Include ASCII wireframes **only when helpful** for understanding the UI:

```markdown
## UI Wireframe Concepts

### Page Name

┌─────────────────────────────────────┐
│ Component layout... │
└─────────────────────────────────────┘
```

### 8. Definition of Done

A final checklist that MUST include testing requirements. Reference the coding-standards skill but also include spec-specific items:

```markdown
## Definition of Done

- [ ] All acceptance criteria are met
- [ ] Unit tests cover core logic
- [ ] Integration tests verify data persistence (if applicable)
- [ ] Component tests (bUnit) cover UI interactions (if applicable)
- [ ] UI has been verified using Playwright (if UI changes)
- [ ] Code reviewed and merged to main branch
```
