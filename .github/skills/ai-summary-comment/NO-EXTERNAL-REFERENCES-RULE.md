# Critical Rule: No External File References in PR Comments

## The Problem

When the PR agent posts review comments to GitHub, those comments are viewed by:
- PR authors
- Other reviewers  
- Future contributors searching issue history
- Community members watching the PR

**None of these people have access to your local files!**

## The Rule

### ❌ NEVER Do This

```markdown
### Title & Description: ⚠️ Minor Updates Needed

**Issues to fix:**
1. Missing required NOTE block
2. Technical inaccuracy in description

**See:** `CustomAgentLogsTmp/PRState/27340/pr-finalize/pr-finalize-summary.md` for recommended updates
```

**Why this is bad:**
- GitHub users can't access `CustomAgentLogsTmp/`
- Makes the review useless - "see file I can't access"
- Author can't act on recommendations
- Future agents can't learn from the review

---

### ✅ ALWAYS Do This

```markdown
### Title & Description: ⚠️ Minor Updates Needed

**Current description is HIGH QUALITY:**
- ✅ Clear root cause for both platforms
- ✅ Before/after videos
- ✅ Well-structured sections

**Issues to fix:**

**Issue 1: Missing required NOTE block**

Add this at the top of the description:
```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

**Issue 2: Technical inaccuracy in "Description of Change"**

Current text says:
> "Added platform-specific handling to dismiss the soft keyboard **and remove focus**..."

Should say:
> "Added platform-specific handling to dismiss the soft keyboard when the Entry or Editor visibility is set to False."

**Reason:** The code only calls `HideSoftInputAsync()` to dismiss the keyboard. It does NOT call `Unfocus()` to remove focus. Focus state remains unchanged.

**Recommended additions:**

Add an **Implementation** subsection:
```markdown
**Implementation:**
- Added `MapIsVisible` handler in `InputView.Platform.cs` (iOS/Android only)
- Calls `HideSoftInputAsync()` when control becomes invisible and keyboard is showing
- Registered handler in `Entry.Mapper.cs` and `Editor.Mapper.cs`
```
```

**Why this is good:**
- ✅ Self-contained - everything needed is in the comment
- ✅ Actionable - author can copy/paste the fixes
- ✅ Clear - shows exact before/after text
- ✅ Educational - explains the reasoning
- ✅ Accessible - works on GitHub

---

## Where This Applies

### pr-finalize Skill

When running `pr-finalize` skill, you create TWO outputs:

1. **Summary file** (local reference)
   - Location: `CustomAgentLogsTmp/PRState/XXXXX/pr-finalize/pr-finalize-summary.md`
   - Purpose: Your detailed analysis and working notes
   - Audience: You and local CLI users

2. **State file Report section** (GitHub audience)
   - Location: `CustomAgentLogsTmp/PRState/pr-XXXXX.md` (Report phase)
   - Purpose: Final recommendations that get posted to GitHub
   - Audience: PR authors, reviewers, community
   - **MUST be self-contained** - no external references

### PR Agent Phase 4 (Report)

When completing Phase 4:
- Include ALL pr-finalize findings inline
- Show exact code blocks for NOTE block
- Show exact before/after text for corrections
- Explain reasoning for each recommendation
- Never reference local files

---

## Examples from Real Usage

### ❌ Bad Example (PR #27340 - first attempt)

```markdown
**Issues to fix:**
1. **Missing required NOTE block** (mandatory for all PRs)
2. **Minor technical inaccuracy:** Description says "remove focus" but code only dismisses keyboard

**See:** `CustomAgentLogsTmp/PRState/27340/pr-finalize/pr-finalize-summary.md` for recommended updates
```

**Result:** PR author sees the issues but has no idea how to fix them without accessing local files.

---

### ✅ Good Example (PR #27340 - corrected)

```markdown
**Issues to fix:**

**Issue 1: Missing required NOTE block**

Add this at the top of the description:
```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

**Issue 2: Technical inaccuracy in "Description of Change"**

Current text says:
> "Added platform-specific handling to dismiss the soft keyboard **and remove focus**..."

Should say:
> "Added platform-specific handling to dismiss the soft keyboard when the Entry or Editor visibility is set to False."

**Reason:** The code only calls `HideSoftInputAsync()` to dismiss the keyboard. It does NOT call `Unfocus()` to remove focus.

**Recommended additions to description:**

Add an **Implementation** subsection after "Description of Change":
```markdown
**Implementation:**
- Added `MapIsVisible` handler in `InputView.Platform.cs` (iOS/Android only)
- Calls `HideSoftInputAsync()` when control becomes invisible and keyboard is showing
- Registered handler in `Entry.Mapper.cs` and `Editor.Mapper.cs`
```
```

**Result:** PR author can immediately act on every recommendation with clear guidance.

---

## Checklist for Report Phase

When completing Phase 4, verify:

- [ ] All recommendations are inline (no file references)
- [ ] Code blocks show exact text to add
- [ ] Before/after comparisons for corrections
- [ ] Reasoning explained for each issue
- [ ] Examples are copy/paste ready
- [ ] No references to `CustomAgentLogsTmp/`

---

## Quick Reference

| What | Where | Audience | Self-Contained? |
|------|-------|----------|-----------------|
| Summary file | `CustomAgentLogsTmp/.../summary.md` | Local CLI | N/A (local only) |
| State file | `CustomAgentLogsTmp/PRState/pr-XXXXX.md` | GitHub users | ✅ YES - REQUIRED |
| PR comment | GitHub PR page | Public | ✅ YES - REQUIRED |

**Remember:** Anything that goes in the state file's `<details>` sections will be posted to GitHub. Make it self-contained!

--
