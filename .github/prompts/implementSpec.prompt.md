---
name: implementSpec
description: Implement specs by providing the numbers of them (ideally one at a time).
---

# Spec Execution

## Workflow

When implementing a spec:

### 1. Create a Todo List

Use `manage_todo_list` to create a structured list of tasks based on the spec's acceptance criteria. Each acceptance criterion should become a todo item.

```
Example:
- [ ] Add CLI argument parsing for input files
- [ ] Implement data normalization pipeline
- [ ] Create domain validation rules
- [ ] Add retry logic for transient failures
- [ ] Write tests for all features
- [ ] Verify end-to-end flow using integration tests
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
- [ ] End-to-end behavior is verified with integration tests
- [ ] Spec file is updated with checked boxes
- [ ] Status is changed to ✅ Complete

## Mark as complete

Finally, if the above requirements are met, fill in the checkboxes in the spec file and mark it as complete.

## README Updates

**When a spec adds new user-facing features to the UI**, update the README.md file to document the new functionality. The README contains a "Pages" section that describes the features available on each page.

- Only update the README for **user-visible feature additions**
- Bug fixes, refactoring, and internal changes do not require README updates
- Add new pages to the README if they are created
- Update existing page descriptions if new features are added to them
