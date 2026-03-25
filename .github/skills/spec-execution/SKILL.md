---
name: spec-execution
description: Guidelines for implementing features based on specification documents. Use this skill when the user asks you to implement a spec, work through a spec, or complete a feature defined in a spec file.
---

# Spec Execution

## Workflow

When implementing a spec:

### 1. Create a Todo List

Use `manage_todo_list` to create a structured list of tasks based on the spec's acceptance criteria. Each acceptance criterion should become a todo item.

```
Example:
- [ ] Implement file upload UI
- [ ] Add bank auto-detection
- [ ] Create transaction preview table
- [ ] Add duplicate detection logic
- [ ] Write tests for all features
- [ ] Verify UI with Playwright
```

### 2. Work Through Items Sequentially

- Mark each todo as **in-progress** before starting work
- Mark each todo as **completed** immediately after finishing
- Only have **one item in-progress** at a time

### 3. Update the Spec File When Complete

Once all acceptance criteria are implemented:

1. Change the spec's **Status** from `📋 Not Started` or `🚧 In Progress` to `✅ Complete`
2. Check all the acceptance criteria checkboxes (`- [ ]` → `- [x]`)
3. If only partial implementation, note which items remain unchecked

## Completion Checklist

Before marking a spec as complete, ensure:

- [ ] All acceptance criteria are implemented
- [ ] Tests are written for all features
- [ ] UI changes are verified with Playwright
- [ ] Spec file is updated with checked boxes
- [ ] Status is changed to ✅ Complete

## README Updates

**When a spec adds new user-facing features to the UI**, update the README.md file to document the new functionality. The README contains a "Pages" section that describes the features available on each page.

- Only update the README for **user-visible feature additions**
- Bug fixes, refactoring, and internal changes do not require README updates
- Add new pages to the README if they are created
- Update existing page descriptions if new features are added to them
