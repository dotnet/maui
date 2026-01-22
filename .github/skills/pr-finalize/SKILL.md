---
name: pr-finalize
description: Verifies PR title and description match actual implementation. Works on any PR. Optionally updates agent session markdown if present.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh)
---

# PR Finalize

Ensures PR title and description accurately reflect the implementation for a good commit message.

**Works on any PR** - Does not require agent involvement or session markdown.

## Inputs

| Input | Required | Source |
|-------|----------|--------|
| PR number | Yes | User provides |
| Session markdown | Optional | `.github/agent-pr-session/issue-XXXXX.md` or `pr-XXXXX.md` |

## Outputs

1. **Recommended PR title** - Platform prefix, behavior-focused
2. **Recommended PR description** - NOTE block + structured content
3. **Session markdown updates** (only if file exists)

## Completion Criteria

- [ ] Reviewed PR diff to understand actual implementation
- [ ] Verified title matches implementation (or recommended fix)
- [ ] Verified description matches implementation (or recommended fix)
- [ ] Checked for session markdown and updated if needed

## When to Use

- Before merging any PR
- "Finalize PR #XXXXX"
- "Check PR description for #XXXXX"
- When implementation changed during review

## When NOT to Use

- ❌ For analyzing lessons learned (use `learn-from-pr` skill instead)
- ❌ For writing or running tests (use `write-tests` or sandbox)
- ❌ For investigating why PR build failed (use `pr-build-status`)

## Constraints

- **Don't change PR title/description directly** - Only recommend changes
- **Don't modify code** - Only verify title/description accuracy
- **Match existing style** - Follow PR template format
- **Preserve user content** - Enhance, don't replace custom descriptions

---

## Workflow

### Step 1: Get PR State

```bash
gh pr view XXXXX --json title,body,files
gh pr diff XXXXX
gh pr view XXXXX --json commits --jq '.commits[].messageHeadline'
```

### Step 2: Analyze Implementation

Read the diff and understand:
- What was actually changed (not what was planned)
- Which platforms are affected
- What the fix does

### Step 3: Verify Title

| Requirement | Good | Bad |
|-------------|------|-----|
| Platform prefix (if specific) | `[iOS] Fix Shell back button` | `Fix Shell back button` |
| Describes behavior | `Fix long-press not triggering events` | `Fix #23892` |
| No noise prefixes | `[iOS] Fix...` | `[PR agent] Fix...` |

### Step 4: Verify Description

Must include:
1. NOTE block (for testing PR artifacts)
2. Description of Change (matches actual implementation)
3. Issues Fixed

Should include (for agent context):
- Root cause
- Fix approach
- Key insight

### Step 5: Session Markdown (If Exists)

```bash
ls .github/agent-pr-session/issue-XXXXX.md .github/agent-pr-session/pr-XXXXX.md 2>/dev/null
```

**If file exists:**
- Check if "Selected Fix: [PENDING]" → update with actual fix
- Add "ACTUAL IMPLEMENTED FIX" section if missing
- Document lessons learned

**If file doesn't exist:** Skip this step.

### Step 6: Present Recommendations

Output recommended title and description.

---

## Error Handling

| Situation | Action |
|-----------|--------|
| PR not found | Ask user to verify PR number |
| No session markdown | Proceed - only verify title/description |
| Title already good | Confirm it's good, no change needed |
| Description missing | Generate recommended description |

---

## Output Template

```markdown
# PR Finalize: #XXXXX

## Title Assessment
**Current:** [current title]
**Recommendation:** [recommended title, or "✅ Current title is good"]

## Description Assessment
**Issues:**
- [issue 1]
- [issue 2]

**Recommended Description:**

<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

### Description of Change

[Brief summary matching implementation]

**Root cause:** [Why bug occurred]

**Fix:** [What code now does]

### Issues Fixed

Fixes #XXXXX

## Session Markdown
[Updated / No file exists / Already complete]
```

---

## Title Requirements

- **Platform prefix** if platform-specific: `[iOS]`, `[Android]`, `[Windows]`, `[MacCatalyst]`
- **Behavior-focused** - Describe what changed, not issue number
- **Concise** - Should fit in commit message subject line

## Description Requirements

### Required Sections

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

### Recommended Additions (for agent context)

| Element | Purpose |
|---------|---------|
| **Root cause** | Why the bug occurred |
| **Fix approach** | What the code now does |
| **Key insight** | Non-obvious understanding |
| **What to avoid** | Patterns that would re-break |

---

## Session Markdown Updates (Optional)

If session markdown exists and is incomplete, add:

```markdown
## ACTUAL IMPLEMENTED FIX

**Selected Fix:** [Brief description]

**What was implemented:**
1. [What changed]
2. [Key files]

**Key insight:** [Non-obvious understanding]

## Lessons Learned

**What would have helped:**
1. [Suggestion 1]
2. [Suggestion 2]
```

---

## Common Issues

| Problem | Solution |
|---------|----------|
| Description doesn't match code | Rewrite from actual diff |
| Missing root cause | Add from issue/analysis |
| References wrong approach | Update to describe final approach |
| "Selected Fix: [PENDING]" | Document actual implementation |

---

## Example

**PR #33352** - MacCatalyst crash on window disposal

**Step 1: Get PR State**
```bash
gh pr view 33352 --json title,body,files
# Title: "Fix ObjectDisposedException in PageViewController"
# Files: PageViewController.cs
```

**Step 2: Analyze Implementation**
- Fix adds null check for window.Handler before accessing services
- Platform: MacCatalyst specific
- Prevents crash during TraitCollectionDidChange when window disposed

**Step 3: Verify Title**
- Current: "Fix ObjectDisposedException in PageViewController"
- Assessment: Missing platform prefix
- Recommended: `[MacCatalyst] Fix ObjectDisposedException in PageViewController.TraitCollectionDidChange`

**Step 4: Verify Description**
- Missing NOTE block
- Missing root cause explanation
- Generate recommended description

**Output:**
```markdown
# PR Finalize: #33352

## Title Assessment
**Current:** Fix ObjectDisposedException in PageViewController
**Recommendation:** [MacCatalyst] Fix ObjectDisposedException in PageViewController.TraitCollectionDidChange

## Description Assessment
**Issues:**
- Missing NOTE block for testing PR artifacts
- Missing root cause explanation

**Recommended Description:**
[Full description with NOTE block, root cause, fix approach...]

## Session Markdown
Updated - Added "ACTUAL IMPLEMENTED FIX" section
```

---

## Integration

- **pr-finalize** → Verify PR ready to merge (this skill)
- **learn-from-pr** → Extract lessons after finalization
- **learn-from-pr agent** → Extract lessons AND apply improvements
