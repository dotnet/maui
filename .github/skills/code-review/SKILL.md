---
name: code-review
description: Reviews PR code changes for correctness, safety, and consistency with MAUI conventions. Uses independence-first assessment (code before narrative) and optional multi-model review. Invoke between Gate and Fix to triage, or after Fix to compare candidates.
---

# Code Review Skill

Evaluates code changes for correctness, safety, performance, and consistency with .NET MAUI conventions. Designed to plug into the PR agent workflow at two points:

1. **Pre-Fix Triage** — After Gate passes, quickly assess whether the PR's fix is good enough to skip the expensive Fix phase
2. **Post-Fix Comparison** — After try-fix exploration, compare candidates on quality dimensions beyond pass/fail

## Core Principles

1. **Independence-first** — Form your assessment from the code BEFORE reading the PR description. This prevents anchoring on the author's framing
2. **Full-context** — Read entire source files, not just diffs. Check callers, consumers, and git history
3. **Empirical grounding** — Reference specific code, line numbers, and call sites. No vague concerns
4. **Severity calibration** — Distinguish errors from warnings from suggestions. Not everything is critical
5. **Devil's advocate** — Challenge your own conclusions before finalizing

## Inputs

| Input | Required | Description |
|-------|----------|-------------|
| mode | Yes | `triage` (Pre-Fix) or `compare` (Post-Fix) |
| pr_number | Yes | GitHub PR number |
| candidates | Compare only | List of passing fix candidates (from Fix Candidates table) with diffs |
| state_file | Optional | Path to PR agent state file |

## Outputs

| Field | Description |
|-------|-------------|
| `verdict` | `LGTM`, `NEEDS_REVIEW`, `NEEDS_CHANGES`, or `SKIP_FIX_PHASE` |
| `confidence` | `high`, `medium`, or `low` |
| `findings` | Categorized findings with severity levels |
| `recommendation` | What to do next (proceed to Fix, skip Fix, select candidate N, etc.) |

---

## Mode 1: Pre-Fix Triage

**When:** After Gate passes (tests FAIL without fix, PASS with fix), before starting Fix phase.

**Purpose:** Determine if the PR's fix is high-quality enough to skip the expensive multi-model Fix exploration.

### Workflow

#### Step 1: Gather Code Context (No PR Narrative)

**Do NOT read the PR description or issue yet.**

1. **Get the diff:**
   ```bash
   gh pr diff <PR_NUMBER>
   ```

2. **Read full source files** for every changed file (not just diff hunks):
   ```bash
   gh pr diff <PR_NUMBER> --name-only
   # Then read each file in full
   ```

3. **Check callers and consumers** of changed methods/properties:
   - Use LSP `findReferences` and `incomingCalls` for modified symbols
   - Understand how the changed code is used

4. **Review git history** of changed files:
   ```bash
   git log --oneline -10 -- <changed-file>
   ```

#### Step 2: Form Independent Assessment

Based ONLY on the code (no PR description), answer:

1. **What does this change do?** Describe the behavioral change in your own words
2. **Why might it be needed?** Infer motivation from the code
3. **Is the approach sound?** Would a simpler alternative work?
4. **What problems do you see?** Bugs, edge cases, thread safety, missing validation

#### Step 3: Read PR Narrative and Reconcile

Now read the PR description, linked issue, and comments. Treat as claims to verify.

1. Where your assessment disagrees with the author's claims, investigate further
2. If the PR claims a bug fix, verify the root cause analysis matches the code
3. Check existing review comments to avoid duplicating feedback

#### Step 4: Triage Verdict

**SKIP_FIX_PHASE** — The fix is correct, minimal, well-targeted, and handles edge cases. No value in exploring alternatives.

**Criteria for SKIP_FIX_PHASE (ALL must be true):**
- Fix addresses root cause, not symptoms
- Minimal change footprint (no unnecessary modifications)
- Handles edge cases identified in your review
- Consistent with MAUI codebase patterns
- No thread-safety, lifecycle, or performance concerns
- Your confidence is `high`

**NEEDS_REVIEW** — The fix works (Gate proved it) but has quality concerns. Proceed to Fix phase to explore alternatives.

**Criteria for NEEDS_REVIEW (ANY is sufficient):**
- Fix addresses symptoms rather than root cause
- Unnecessarily complex for the problem
- Missing edge case handling
- Inconsistent with codebase patterns
- Thread-safety or lifecycle concerns
- Your confidence is `medium` or `low`

---

## Mode 2: Post-Fix Comparison

**When:** After Fix phase completes, before Report phase. Multiple candidates passed tests.

**Purpose:** Compare passing candidates on quality dimensions beyond "tests pass."

### Workflow

#### Step 1: Read Each Candidate's Diff

For each passing candidate (PR fix + any try-fix passes):
1. Read the full diff (`fix.diff` from try-fix output, or `gh pr diff` for PR)
2. Read the full source files affected
3. Understand each approach's strategy

#### Step 2: Evaluate Each Candidate

Score each candidate on these dimensions:

| Dimension | Question |
|-----------|----------|
| **Root Cause** | Does it fix the root cause or mask symptoms? |
| **Simplicity** | How many files/lines changed? Is it the minimal fix? |
| **Robustness** | Does it handle edge cases? Could it regress? |
| **Safety** | Thread-safety? Lifecycle correctness? Resource leaks? |
| **Consistency** | Does it match existing patterns in the codebase? |
| **Maintainability** | Will future developers understand this change? |

#### Step 3: Devil's Advocate

For your top-ranked candidate:
- What edge cases might still break?
- What if the test is too narrow and doesn't cover real usage?
- Could this fix cause regressions in other scenarios?

For your lowest-ranked candidate:
- Could it actually be better in ways you're not seeing?
- Is its simplicity an advantage you're underweighting?

#### Step 4: Comparison Verdict

Produce a ranked comparison:

```markdown
## Code Review: Fix Comparison

### Rankings

| Rank | Candidate | Approach | Strengths | Concerns |
|------|-----------|----------|-----------|----------|
| 1 | PR #XXXXX | [approach] | [strengths] | [concerns] |
| 2 | try-fix #3 | [approach] | [strengths] | [concerns] |
| 3 | try-fix #1 | [approach] | [strengths] | [concerns] |

### Recommendation
**Selected:** [Candidate] — [1-2 sentence justification]
**Confidence:** high/medium/low
```

---

## Multi-Model Review (Optional)

When the environment supports multiple models, run the review in parallel for diverse perspectives. Different models catch different classes of issues.

**When to use:** Pre-Fix Triage mode only (Post-Fix Comparison is fast enough single-model).

**How:**
1. Select 2-3 models from different families (see SHARED-RULES.md for available models)
2. Launch sub-agents in parallel with the same review prompt
3. Synthesize: deduplicate findings, elevate issues flagged by multiple models
4. Present unified review noting which findings had multi-model agreement

**Timeout:** If a sub-agent hasn't completed after 5 minutes, proceed with available results.

---

## MAUI-Specific Review Rules

### Platform Code

- **Check platform-specific file extensions**: `.android.cs`, `.ios.cs`, `.maccatalyst.cs`, `.windows.cs`
- **Both `.ios.cs` and `.maccatalyst.cs` compile for MacCatalyst** — no precedence mechanism
- **Handler lifecycle**: Register in `ConnectHandler`, unregister in `DisconnectHandler`
- **Dispose `Java.Lang.Object` derivatives** on Android to prevent memory leaks
- **Use type aliases** for namespace collisions: `using AView = Android.Views.View;`

### Common MAUI Pitfalls

| Pitfall | What to Check |
|---------|---------------|
| **Handler lifecycle** | Does `DisconnectHandler` clean up everything `ConnectHandler` registered? |
| **Platform threading** | Are Android View APIs called on UI thread? (`platformView.Post()`) |
| **Mapper vs Property** | Is the right mechanism used? Mappers for platform sync, properties for cross-platform |
| **Null platform view** | Does the handler guard against `PlatformView` being null after disconnect? |
| **SafeArea** | Does the fix account for safe area insets on iOS/Android? |
| **CollectionView handler** | Items2/ for iOS/MacCatalyst, Items/ for Android/Windows (see handler detection instructions) |

### What NOT to Flag

- Style/formatting issues (CI catches these via `dotnet format`)
- Missing XML docs on non-public APIs
- Test naming conventions (unless egregiously unclear)
- Minor performance micro-optimizations in non-hot paths

---

## Review Output Format

### Pre-Fix Triage Output

```markdown
## Code Review: Pre-Fix Triage — PR #XXXXX

### Independent Assessment
**What this changes:** [Your understanding from code alone]
**Inferred motivation:** [Why this change seems needed]

### Reconciliation with PR Narrative
**Author claims:** [Summary of PR description]
**Agreement/disagreement:** [Where your assessment matches or differs]

### Findings

#### ❌ Error — [Brief description]
[Explanation with specific code references]

#### ⚠️ Warning — [Brief description]
[Explanation with specific code references]

#### 💡 Suggestion — [Brief description]
[Explanation]

### Verdict: SKIP_FIX_PHASE / NEEDS_REVIEW
**Confidence:** high/medium/low
**Reasoning:** [2-3 sentences]
**Recommendation:** [Skip Fix phase / Proceed to Fix phase with hints]
```

### Post-Fix Comparison Output

```markdown
## Code Review: Fix Comparison — PR #XXXXX

### Candidate Analysis

#### PR Fix: [approach]
- **Root cause:** ✅/⚠️/❌
- **Simplicity:** ✅/⚠️/❌
- **Robustness:** ✅/⚠️/❌
- **Safety:** ✅/⚠️/❌

#### try-fix #N: [approach]
- **Root cause:** ✅/⚠️/❌
- **Simplicity:** ✅/⚠️/❌
- **Robustness:** ✅/⚠️/❌
- **Safety:** ✅/⚠️/❌

### Devil's Advocate
[Challenges to your top-ranked candidate]

### Recommendation
**Selected:** [Candidate] — [justification]
**Confidence:** high/medium/low
```

---

## Verdict Consistency Rules

1. **The verdict must reflect your most severe finding.** If you have any ⚠️ findings, the verdict cannot be `SKIP_FIX_PHASE`.
2. **When uncertain, proceed to Fix.** `SKIP_FIX_PHASE` requires `high` confidence. If you're unsure, use `NEEDS_REVIEW` and let Fix phase explore alternatives.
3. **Devil's advocate before finalizing.** Re-read all findings. For each warning, ask: "Would I be comfortable if this merged as-is?"
4. **Never approve what you can't verify.** If the fix touches platform code you can't fully reason about, say so explicitly.

---

## Completion Criteria

- [ ] Full source files read (not just diffs)
- [ ] Independent assessment formed before reading PR narrative
- [ ] Findings categorized by severity
- [ ] Devil's advocate check performed
- [ ] Verdict is consistent with findings
- [ ] Output follows the format above
- [ ] Results reported to invoker (or saved to state file)
