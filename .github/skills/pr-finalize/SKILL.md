---
name: pr-finalize
description: Finalizes any PR for merge by verifying title/description match implementation AND performing multi-model code review for best practices. Use when asked to "finalize PR", "check PR description", "review commit message", before merging any PR, or when PR implementation changed during review. Do NOT use for extracting lessons (use learn-from-pr), writing tests (use write-tests-agent), or investigating build failures (use pr-build-status).
---

# PR Finalize

Ensures PR title and description accurately reflect the implementation, and performs a **multi-model code review** for best practices before merge.

**Standalone skill** - Can be used on any PR, not just PRs created by the pr agent.

## Two-Phase Workflow

1. **Title & Description Review** - Verify PR metadata matches implementation
2. **Multi-Model Code Review** - Get consensus from 5 models on best practices

---

## Phase 1: Title & Description

### Core Principle: Preserve Quality

**Review existing description BEFORE suggesting changes.** Many PR authors write excellent, detailed descriptions. Your job is to:

1. **Evaluate first** - Is the existing description good? Better than a template?
2. **Preserve quality** - Don't replace a thorough description with a generic template
3. **Enhance, don't replace** - Add missing required elements (NOTE block, issue links) without rewriting good content
4. **Only rewrite if needed** - When description is stale, inaccurate, or missing key information

## Usage

```bash
# Get current state (no local checkout required)
gh pr view XXXXX --json title,body
gh pr view XXXXX --json files --jq '.files[].path'

# Review commit messages (helpful for squash/merge commit quality)
gh pr view XXXXX --json commits --jq '.commits[].messageHeadline'

# Review actual code changes
gh pr diff XXXXX

# Optional: if the PR branch is checked out locally
git diff origin/main...HEAD
```

## Evaluation Workflow

### Step 1: Review Existing Description Quality

Before suggesting changes, evaluate the current description:

| Quality Indicator | Look For |
|-------------------|----------|
| **Structure** | Clear sections, headers, organized flow |
| **Technical depth** | File-by-file changes, specific code references |
| **Scanability** | Easy to find what changed and where |
| **Accuracy** | Matches actual diff - not stale or incorrect |
| **Completeness** | Platforms, breaking changes, testing info |

### Step 2: Compare to Template

Ask: "Is the existing description better than what my template would produce?"

- **If YES**: Keep existing, only add missing required elements
- **If NO**: Suggest improvements or replacement

### Step 3: Produce Output

- Recommended PR title (if change needed)
- Assessment of existing description
- Specific additions needed (e.g., "Add NOTE block at top")
- Only full replacement if description is inadequate

## Title Requirements

**The title becomes the commit message headline.** Make it searchable and informative.

| Requirement | Good | Bad |
|-------------|------|-----|
| Platform prefix (if specific) | `[iOS] Fix Shell back button` | `Fix Shell back button` |
| Describes behavior, not issue | `[iOS] SafeArea: Return Empty for non-ISafeAreaView views` | `Fix #23892` |
| Captures the "what" | `Return Empty for non-ISafeAreaView` | `Fix SafeArea bug` |
| Notes model change if applicable | `(opt-in model)` | (omitted) |
| No noise prefixes | `[iOS] Fix...` | `[PR agent] Fix...` |

### Title Formula

```
[Platform] Component: What changed (model change if any)
```

Examples:
- `[iOS] SafeArea: Return Empty for non-ISafeAreaView views (opt-in model)`
- `[Android] CollectionView: Fix scroll position reset on item update`
- `[Windows] Shell: Use NavigationView instead of custom flyout`

## Description Requirements

PR description should:
1. Start with the required NOTE block (so users can test PR artifacts)
2. Include the base sections from `.github/PULL_REQUEST_TEMPLATE.md` ("Description of Change" and "Issues Fixed"). The skill adds additional structured fields (Root cause, Fix, Key insight, etc.) as recommended enhancements for better agent context.
3. Match the actual implementation

```markdown
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Description of Change
[Must match actual implementation]

### Issues Fixed
Fixes #XXXXX
```

## Content for Future Agents

**The title and description become the commit message.** Future agents searching git history will use this to understand:
- What changed and why
- What patterns to follow or avoid
- How this change affects related code

### Required Elements for Agent Success

| Element | Purpose | Example |
|---------|---------|---------|
| **Root cause** | Why the bug occurred | "Non-ISafeAreaView views falling through to return baseSafeArea" |
| **Fix approach** | What the code now does | "Return SafeAreaPadding.Empty for views without interface" |
| **Philosophy/model change** | If behavior model changed | "Before: opt-out. After: opt-in via interface" |
| **Key interfaces/types** | Types agents need to know | "ISafeAreaView, ISafeAreaView2 = opt-in contract" |
| **What NOT to do** | Failed approaches to avoid | "Don't use Element type in Platform layer" |
| **Architectural constraints** | Layer boundaries, type availability | "Platform layer cannot reference Controls types" |
| **Edge cases** | Known limitations or risks | "Legacy layouts are [Obsolete], custom views need interface" |

### "What NOT to Do" Section (Critical)

When try-fix or debugging revealed failed approaches, document them:

```markdown
### What NOT to Do (for future agents)

- ❌ **Don't use [Type] in [Layer]** - [Why it fails]
- ❌ **Don't use [Pattern]** - [Why it's brittle/wrong]
- ❌ **Don't [Approach]** - [Why it doesn't work]
```

This prevents future agents from repeating failed experiments.

### Philosophy/Model Changes

When a fix changes the behavioral model (not just fixing a bug), call it out explicitly:

```markdown
**This is a philosophy change:**
- **Before:** [Old behavior model]
- **After:** [New behavior model]
```

Example: "Before: Safe area applied by default (opt-out). After: Only views implementing ISafeAreaView get safe area (opt-in)."

## Common Issues

| Problem | Cause | Solution |
|---------|-------|----------|
| Description doesn't match code | Implementation changed during review | Update description to match actual diff |
| Missing root cause | Author focused on "what" not "why" | Add root cause from issue/analysis |
| References wrong approach | Started with A, switched to B | Update to describe final approach |
| Missing NOTE block | Author didn't use template | Prepend NOTE block, keep rest |
| Good description replaced | Agent used template blindly | Evaluate existing quality first |

## Output Format

### When Existing Description is Good

```markdown
## PR #XXXXX Finalization Review

### ✅ Title: [Good / Needs Update]
**Current:** "Existing title"
**Recommended:** "[Platform] Improved title" (if needed)

### ✅ Description: Excellent - Keep As-Is

**Quality assessment:**
- Structure: ✅ Clear sections with headers
- Technical depth: ✅ File-by-file breakdown
- Accuracy: ✅ Matches implementation
- Completeness: ✅ Platforms, breaking changes noted

**Only addition needed:**
- ❌ Missing NOTE block - prepend to top

**Action:** Add NOTE block, preserve everything else.
```

### When Description Needs Rewrite

Use structured template only when existing description is inadequate:

```markdown
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Root Cause

[Why the bug occurred - be specific about the code path]

### Description of Change

[What the code now does]

**This is a philosophy change:** (if applicable)
- **Before:** [Old model]
- **After:** [New model]

[Cross-platform alignment notes if relevant]

### Key Technical Details

**[Relevant interfaces/types]:**
- `InterfaceA` - [What it does]
- `InterfaceB` - [What it does]

**[Category] that [work/don't work]:**
- List of types/views affected

### What NOT to Do (for future agents)

- ❌ **Don't [approach 1]** - [Why it fails]
- ❌ **Don't [approach 2]** - [Why it's wrong]
- ❌ **Don't [approach 3]** - [Constraint that prevents it]

### Edge Cases

| Scenario | Risk | Mitigation |
|----------|------|------------|
| [Case 1] | Low/Medium/High | [How to handle] |
| [Case 2] | Low/Medium/High | [How to handle] |

### Issues Fixed

Fixes #XXXXX

### Platforms Tested

- [x] iOS
- [x] Android
- [ ] Windows
- [ ] Mac
```

## Quality Comparison Examples

### Good Existing Description (KEEP)

```markdown
## Changes Made

### 1. **PickerHandler.iOS.cs** - MacCatalyst-specific improvements

#### Added UIAlertController instance field
- Declared `UIAlertController? pickerController` as instance field...

#### Improved picker dismiss logic
- Moved picker dismiss logic from event handler to "Done" button action
- Removed `EditingDidEnd` event handler causing duplicate dismiss calls

## Platforms Affected
- **MacCatalyst** (primary)
- iOS (no behavior changes, shared code)

## Breaking Changes
None
```

**Verdict:** Excellent - file-by-file breakdown, specific changes, platforms, breaking changes. Keep it.

### Poor Existing Description (REWRITE)

```markdown
Fixed the issue mentioned in #30897
```

**Verdict:** Inadequate - no detail on what changed. Use template.

---

## Phase 2: Multi-Model Code Review

After verifying title/description, perform a **multi-model code review** to catch best practice violations and edge cases before merge.

### Why Multi-Model?

Different models catch different issues. Consensus from 5 models provides:
- Higher confidence in findings (multiple models agree)
- Broader coverage of edge cases
- Reduced false positives (single-model quirks filtered out)

### Models to Consult

Use 5 diverse models for best coverage:

| Model | Strengths |
|-------|-----------|
| `claude-sonnet-4` | Balanced analysis, good code patterns |
| `claude-opus-4.5` | Deep reasoning, edge cases |
| `gpt-5.2` | Practical recommendations |
| `gpt-5.2-codex` | Code-specific expertise |
| `gemini-3-pro-preview` | Alternative perspective |

### Review Prompt Template

Send the same prompt to all 5 models via `task` tool with `model` parameter:

```
Review the following code changes from PR #XXXXX for best practices. Focus on:
1. Code quality and maintainability
2. Error handling and edge cases  
3. Performance implications
4. Any potential improvements

[Include relevant code snippets from PR diff]

Provide specific, actionable recommendations. Be concise.
```

### Execution

```csharp
// Invoke 5 models in parallel using task tool
task(agent_type: "general-purpose", model: "claude-sonnet-4", prompt: "<review prompt>")
task(agent_type: "general-purpose", model: "claude-opus-4.5", prompt: "<review prompt>")
task(agent_type: "general-purpose", model: "gpt-5.2", prompt: "<review prompt>")
task(agent_type: "general-purpose", model: "gpt-5.2-codex", prompt: "<review prompt>")
task(agent_type: "general-purpose", model: "gemini-3-pro-preview", prompt: "<review prompt>")
```

### Synthesize Consensus

After receiving all 5 responses, synthesize findings by agreement level:

| Agreement | Classification | Action |
|-----------|----------------|--------|
| **4-5 models agree** | 🔴 Critical | Must fix before merge |
| **3 models agree** | 🟡 High Priority | Should fix |
| **2 models agree** | 🟢 Minor | Nice to have |
| **1 model only** | ⚪ Skip | Likely false positive |

### Output Format

```markdown
## Multi-Model Code Review Consensus

### 🔴 Critical Issues (4+ models agree)

**[Issue Title]**
- **Models:** claude-sonnet-4, claude-opus-4.5, gpt-5.2, gemini-3-pro-preview
- **Problem:** [Description]
- **Recommendation:** [Code fix]

### 🟡 High Priority (3 models agree)

**[Issue Title]**
- **Models:** [List]
- **Problem:** [Description]  
- **Recommendation:** [Code fix]

### 🟢 Minor Improvements (2 models agree)

- [Improvement 1]
- [Improvement 2]

### ✅ Positive Feedback (all models agree)

- [Good practice 1]
- [Good practice 2]
```

### When to Post Review

- **Critical issues found**: Post review requesting changes with consensus findings
- **Only minor issues**: Approve with suggestions in comment
- **No issues**: Approve, note that multi-model review passed

### Example gh CLI Commands

```bash
# Request changes (critical issues found)
gh pr review XXXXX --repo dotnet/maui --request-changes --body "$reviewBody"

# Approve with comments (minor issues only)
gh pr review XXXXX --repo dotnet/maui --approve --body "$reviewBody"

# Comment without approval/rejection
gh pr review XXXXX --repo dotnet/maui --comment --body "$reviewBody"
```

---

## Complete Example

See [references/complete-example.md](references/complete-example.md) for a full agent-optimized PR description showing all elements above applied to a real SafeArea fix.
