# Spec Creation

## Overview

This defines how to create feature specification documents for the app. Specs are user-story-driven documents with clear acceptance criteria that tell you when you're done.

## Subagent Usage

When creating spec files, you MUST delegate the writing of each individual spec file to a subagent. This is critical for managing context window size and request costs.

**Workflow:**

1. First, analyze the implementation plan yourself to determine how many specs are needed and what each one covers (title, scope, key acceptance criteria).
2. For each spec file, launch a **subagent** with a focused prompt that includes:
   - The spec number, title, and file path to write to
   - The relevant subset of the implementation plan for that spec
   - The full spec structure template (from this system prompt)
   - Instructions to write the file to disk
3. Each subagent should create exactly ONE spec file and nothing else.

**Why subagents?** Each spec file is an independent unit of work. Delegating to subagents keeps the main agent's context clean and avoids bloating it with the full content of every spec. This saves both context window space and premium request costs.

## File Naming & Location

- **Location**: `specs/`
- **Naming**: `spec-XX-Feature-Name.md` (e.g., `spec-01-Transaction-Import.md`)
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
