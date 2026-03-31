---
name: code-review
description: Reviews PR code changes for correctness, safety, and consistency with MAUI conventions. Uses independence-first assessment (code before narrative) and optional multi-model review. Invoke with "review code for PR #XXXXX" or "code review PR #XXXXX".
---

# Code Review Skill

Standalone skill that evaluates PR code changes for correctness, safety, performance, and consistency with .NET MAUI conventions. Can be invoked directly by users or by other agents/skills.

**Trigger phrases:** "review code for PR #XXXXX", "code review PR #XXXXX", "review this PR's code"

## Core Principles

1. **Independence-first** — Form your assessment from the code BEFORE reading the PR description. This prevents anchoring on the author's framing
2. **Full-context** — Read entire source files, not just diffs. Check callers, consumers, and git history
3. **Empirical grounding** — Reference specific code, line numbers, and call sites. No vague concerns
4. **Severity calibration** — Distinguish errors from warnings from suggestions. Not everything is critical
5. **Devil's advocate** — Challenge your own conclusions before finalizing

## Inputs

| Input | Required | Description |
|-------|----------|-------------|
| pr_number | Yes | GitHub PR number to review |

## Outputs

| Field | Description |
|-------|-------------|
| `verdict` | `LGTM`, `NEEDS_CHANGES`, or `NEEDS_DISCUSSION` |
| `confidence` | `high`, `medium`, or `low` |
| `findings` | Categorized findings with severity levels |

---

## Review Workflow

### Step 1: Gather Code Context (No PR Narrative)

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

### Step 2: Load Review Rules

Read `references/review-rules.md` from this skill's directory. These rules are distilled from real code reviews by senior MAUI maintainers across 142 high-discussion PRs.

### Step 3: Form Independent Assessment

Based ONLY on the code (no PR description), answer:

1. **What does this change do?** Describe the behavioral change in your own words
2. **Why might it be needed?** Infer motivation from the code
3. **Is the approach sound?** Would a simpler alternative work?
4. **What problems do you see?** Run through the review rules AND the MAUI-Specific Review Checklist below

### Step 4: Read PR Narrative and Reconcile

Now read the PR description, linked issue, and comments. Treat these as **claims to verify**, not facts.

1. Where your assessment disagrees with the author's claims, investigate further
2. If the PR claims a bug fix, verify the root cause analysis matches the code
3. Check existing review comments to avoid duplicating feedback

### Step 5: Check CI Status

```bash
gh pr checks <PR_NUMBER>
```

Review CI results. **Never post ✅ LGTM if any required CI check is failing.** If CI is failing:
- If caused by the PR's code changes, flag as ❌ error
- If a known infrastructure issue or pre-existing flake, note it but still use ⚠️
- If the PR description acknowledges the failure, note it in the summary

### Step 6: Devil's Advocate and Verdict

Before finalizing your verdict:

1. **Challenge your findings** — For each issue you flagged, ask: "Am I sure, or am I guessing?"
2. **Challenge your approval** — If you're leaning LGTM, ask: "What could go wrong that I'm not seeing?"
3. **Check platform blind spots** — If the change touches platforms you can't fully reason about, say so explicitly

Then deliver your verdict:

- **`LGTM`** — Code is correct, safe, and consistent with MAUI patterns. Ready for human approval.
- **`NEEDS_CHANGES`** — Concrete issues found that should be addressed before merge.
- **`NEEDS_DISCUSSION`** — Complex tradeoffs or architectural questions that need human judgment.

---

## MAUI-Specific Review Rules

Apply the rules in `references/review-rules.md` to each changed file. The rules are distilled from real code reviews across 142 high-discussion PRs and cover 20 categories:

**Platform & Lifecycle:** Handler lifecycle, platform-specific code, Android, iOS/macCatalyst, Windows
**Architecture:** Memory management, threading, safe area, layout, navigation, CollectionView
**Code Quality:** Error handling, null safety, performance, XAML & bindings, API design
**Ecosystem:** Testing, build & MSBuild, image handling, gestures, accessibility

The rules file also includes a **"What NOT to Flag"** section — respect it to avoid noise.

---

## Multi-Model Review (Optional)

When the environment supports multiple models, run the review in parallel for diverse perspectives. Different model families catch different classes of issues.

**How:**
1. Select 2-3 models from different families (e.g., Claude + GPT + Gemini)
2. Launch sub-agents in parallel, each running the full 4-step workflow above
3. Synthesize: deduplicate findings, elevate issues flagged by multiple models
4. Present unified review noting which findings had multi-model agreement

**Timeout:** If a sub-agent hasn't completed after 5 minutes, proceed with available results.

---

## Review Output Format

**Constraints (from Android team's approach):**
- Only comment on added/modified lines — don't flag pre-existing code
- One issue per comment. If the same issue appears many times, flag once with a note listing all affected files
- **Don't pile on.** 3 important comments > 15 nitpicks
- **Don't flag what CI catches.** Skip compiler errors, formatting the linter will catch, etc.
- **Avoid false positives.** Verify the concern actually applies given full context. If unsure, phrase as a question.

```markdown
## Code Review — PR #XXXXX

### Independent Assessment
**What this changes:** [Your understanding from code alone]
**Inferred motivation:** [Why this change seems needed]

### Reconciliation with PR Narrative
**Author claims:** [Summary of PR description]
**Agreement/disagreement:** [Where your assessment matches or differs]

### Findings

#### ❌ Error — [Brief description]
[Explanation with specific file:line references]

#### ⚠️ Warning — [Brief description]
[Explanation with specific file:line references]

#### 💡 Suggestion — [Brief description]
[Explanation]

### Devil's Advocate
[Challenges to your own conclusions]

### Verdict: LGTM / NEEDS_CHANGES / NEEDS_DISCUSSION
**Confidence:** high / medium / low
**Summary:** [2-3 sentences explaining the verdict]
```

---

## Verdict Consistency Rules

1. **The verdict must match your most severe finding.** If you have any ❌ Error findings, the verdict must be `NEEDS_CHANGES`. If only ⚠️ Warnings, use judgment but explain.
2. **Devil's advocate before finalizing.** Re-read all findings. For each warning, ask: "Would I be comfortable if this merged as-is?"
3. **Never approve what you can't verify.** If the fix touches platform code you can't fully reason about, say so explicitly and use `NEEDS_DISCUSSION`.
4. **LGTM means no ❌ Errors.** You can LGTM with 💡 Suggestions. You can LGTM with ⚠️ Warnings only if you've explained why they're acceptable.
5. **🚨 NEVER use `--approve` or `--request-changes` on GitHub.** Only post comments. Approval is a human decision.

---

## Completion Criteria

- [ ] Full source files read (not just diffs)
- [ ] Independent assessment formed before reading PR narrative
- [ ] MAUI-specific checklist walked through for each applicable section
- [ ] Findings categorized by severity (❌ / ⚠️ / 💡)
- [ ] Devil's advocate check performed
- [ ] Verdict is consistent with findings
- [ ] Output follows the format above
