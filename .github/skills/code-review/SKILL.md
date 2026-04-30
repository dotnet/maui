---
name: code-review
description: >-
  Deep code review of PR changes for correctness, safety, and MAUI conventions.
  Uses independence-first assessment (code before narrative) with 345 lines of
  maintainer-sourced review rules. Triggers on: "review code for PR", "code review PR",
  "analyze code changes", "check PR code quality". Do NOT use for: summarizing PRs,
  describing what changed, general PR questions, running tests, or fixing code.
---

# Code Review Skill

Standalone skill that evaluates PR code changes for correctness, safety, performance, and consistency with .NET MAUI conventions. Can be invoked directly by users or by other agents/skills.

**Trigger phrases:** "review code for PR #XXXXX", "code review PR #XXXXX", "review this PR's code", "analyze code changes in PR", "check PR code quality"

**Do NOT use for:** "what does PR #XXXXX do?", "summarize PR", "describe the changes", or any informational query — just answer those directly without invoking this skill.

> **How this differs from other skills:**
> - **`pr-review`** — End-to-end PR workflow (3 phases: pre-flight with code review, try-fix, report; gate runs separately). Use when you want the full pipeline including test verification and fix attempts. Pre-Flight invokes this skill as a sub-agent for independence-first code analysis.
> - **`pr-finalize`** — Verifies PR title/description match implementation + light code review. Use before merging.
> - **`code-review`** (this skill) — Deep code-only review with MAUI domain rules. Use when you want a thorough code analysis without running tests or modifying the PR.

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

Read `.github/skills/code-review/references/review-rules.md`. These rules are distilled from real code reviews by senior MAUI maintainers across 142 high-discussion PRs.

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

#### 🚨 Prior Review Reconciliation (MANDATORY)

Check for prior reviews on the same PR — from the Copilot PR reviewer bot, other agents, or human reviewers:

```bash
# Get all reviews (use repo-context-aware command; bump truncation to 2000 chars to capture structured findings)
gh pr view <PR_NUMBER> --json reviews --jq '.reviews[] | "Reviewer: \(.author.login) | State: \(.state)\n\(.body[0:2000])\n---"'

# Search PR comments for prior critical findings (case-insensitive — reviewers use "Critical", "critical", "[CRITICAL]")
gh pr view <PR_NUMBER> --json comments --jq '.comments[] | select(.body | ascii_downcase | (contains("critical") or contains("must be fixed") or contains("🔴"))) | .body[0:2000]'
```

**Trust scoping:** The comment scan is intentionally broad to catch agent and bot reviews. To avoid spoofing by arbitrary commenters, weight findings from formal reviews (`reviews[].state == "CHANGES_REQUESTED"`) and known reviewer bots (`copilot-pull-request-reviewer`, `PureWeen`, etc.) more heavily than free-form comments. A drive-by comment containing the word "critical" is not, by itself, a blocker — but a `CHANGES_REQUESTED` review is.

**If prior reviews flagged critical issues, you MUST produce a reconciliation table:**

```markdown
### Prior Finding Reconciliation

| Prior Critical Finding | Source | Current Status | Evidence |
|------------------------|--------|----------------|----------|
| [finding] | [reviewer/pass] | ✅ Fixed / ❌ Unresolved / 🔄 Obsolete | [code ref or explanation] |
```

**Rules:**
- If ANY prior critical finding is **unresolved** → verdict must be `NEEDS_CHANGES`
- If status cannot be determined → mark **unresolved** (default to caution)
- If finding is obsolete (code was removed/rewritten) → say so explicitly with evidence
- **NEVER silently drop or contradict a prior critical finding.** This is the #1 cause of regression approvals — PR #34669 was approved with "high confidence" after earlier passes had correctly identified a critical bug that was never fixed.

### Step 5: Check CI Status

```bash
# Full context — all checks (required + optional)
gh pr checks <PR_NUMBER>

# Hard-gate decision — required checks only (use this for the verdict gate below)
gh pr checks <PR_NUMBER> --required

# Programmatic analysis — valid --json fields are: bucket, completedAt, description,
# event, link, name, startedAt, state, workflow. (`conclusion` is NOT a valid field.)
# `bucket` categorizes `state` into pass/fail/pending/skipping/cancel.
gh pr checks <PR_NUMBER> --json name,state,bucket,workflow
```

Review CI results. **🚨 HARD GATE: Never give `LGTM` if any required CI check is failing or pending.** Use `--required` to scope the gate decision so optional/informational check failures don't force a false `NEEDS_CHANGES`.

| CI State | Allowed Verdict |
|----------|----------------|
| All required checks ✅ green | May proceed to verdict |
| Any required check ❌ red | **`NEEDS_CHANGES`** — document which checks failed and whether related to PR |
| Required checks ⏳ pending | **`NEEDS_DISCUSSION`** — state "CI not yet complete" |
| Only optional/informational checks red | May proceed, note the failures |

**NEVER say "No CI failures detected" or "Clean build" without actually running `gh pr checks`.** False CI claims directly caused PR #34669 (100% UITest failure) to be approved and merged.

**UITest awareness:** UITests do NOT run on PR builds in this repo — they only run post-merge. If the PR modifies HostApp pages, handler/extension code, or platform infrastructure, explicitly note: _"UITests do not run on PR builds. Startup and runtime behavior cannot be verified from CI alone."_

### Step 6: Blast Radius, Failure-Mode Probing, and Verdict

#### Blast Radius Assessment (MANDATORY for infrastructure changes)

**Trigger:** PR modifies any of: handlers, platform extensions, toolbar/navigation code, page registration, static state, `PropertyChanged` subscriptions, or HostApp startup paths.

Answer these questions explicitly in the review:

| Question | Why It Matters |
|----------|---------------|
| Does this code run for ALL instances, or only when the new feature is used? | Toolbar code that wraps ALL items in a Grid broke startup for every page (PR #34669) |
| Does this code run at app startup or page initialization? | Static fields initialized on first access can crash the app before any test page loads |
| Are there new static/shared state fields that affect all pages/windows? | Static `ConcurrentDictionary` survives handler disposal unless explicitly cleaned up |
| Do new `PropertyChanged` subscriptions fire for ALL property changes? | A handler subscribing to all PropertyChanged events burns CPU and can throw if it handles unexpected properties |
| Do HostApp test pages reference resources (images, fonts) that exist? | Missing `envelope.png`, `bell.png` etc. can crash the HostApp during page scan/registration |
| What happens at startup with null/default values for new properties? | New BindableProperty with null default must not cause NullRef in platform code paths |

#### Failure-Mode Probing (MANDATORY)

**Do NOT ask easy rhetorical questions with obvious answers.** Probe genuinely challenging failure modes:

- What happens if this code runs on pages/items that DON'T use the new feature?
- What happens during handler disconnect/reconnect (navigation, Shell tab switch)?
- What happens with null `Parent`, `Handler`, `BindingContext`, or `IconImageSource`?
- What happens if referenced resources (images, fonts) don't exist in the project?
- Can multiple subscriptions accumulate across handler lifecycle (missing unsubscribe)?
- Does static state survive page disposal and get stale?
- What happens if the platform API is unavailable (e.g., a newer iOS API used without an `OperatingSystem.IsIOSVersionAtLeast(...)` guard)?

**Anti-pattern (from PR #34669 review):** The reviewer asked "Should BadgeText be int?" and "Could static dictionaries cause memory pressure?" — these are softballs with obvious "no" answers. Meanwhile the ACTUAL crash (toolbar code running for all items at startup) was never probed.

#### Confidence Calibration

**Base confidence by blast radius:**

| Blast Radius | Max Confidence |
|-------------|----------------|
| Localized change, non-startup, non-infrastructure | May be **high** |
| Platform-specific handler/UI plumbing | Max **medium** |
| Shared infrastructure, startup path, global subscriptions/static state | Max **low** |

**Then cap by evidence:**

| Evidence | Confidence Cap |
|----------|---------------|
| CI red or pending | **NEEDS_CHANGES / NEEDS_DISCUSSION** (no LGTM) |
| No relevant tests run (e.g., UITests skip PR builds) | Max **low** |
| CI green but no UI/integration test coverage | Max **medium** |
| CI green + targeted runtime coverage verified | May increase one level |
| Prior critical findings unresolved | **NEEDS_CHANGES** (no LGTM) |

**NEVER give "Confidence: high" on PRs that modify platform infrastructure with >500 lines unless CI is fully green AND UITests have been verified.**

> **Note — high confidence is intentionally unreachable at PR-review time for this category.** UITests do not run on PR builds (see Step 5), so the "UITests verified" precondition cannot be satisfied during review. This is deliberate after PR #34669: large infrastructure PRs are capped at `medium` until post-merge verification. Do **not** hallucinate UITest verification to satisfy this rule — if the rule cannot be satisfied, the cap stands.

#### Deliver Verdict

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
2. Launch sub-agents in parallel, each running the full 6-step workflow above
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

### Prior Finding Reconciliation
| Prior Critical Finding | Source | Current Status | Evidence |
|---|---|---|---|
| [finding] | [reviewer] | ✅ Fixed / ❌ Unresolved / 🔄 Obsolete | [evidence] |
*(If no prior reviews with critical findings, state "No prior critical findings found.")*

### CI Status
- `maui-pr`: ✅ / ❌ (reason) / ⏳
- UITests: ⚠️ Not run on PR builds
- [Other checks]

### Blast Radius Assessment
*(Required for infrastructure/handler/platform changes; omit for simple fixes)*
- Runs for all instances: [yes/no — explanation]
- Startup impact: [yes/no]
- Static/shared state: [yes/no]
- HostApp resources verified: [yes/no/N/A]

### Findings

#### ❌ Error — [Brief description]
[Explanation with specific file:line references]

#### ⚠️ Warning — [Brief description]
[Explanation with specific file:line references]

#### 💡 Suggestion — [Brief description]
[Explanation]

### Failure-Mode Probing
- [Probe]: [Answer — what actually happens in this scenario]
- [Probe]: [Answer]

### Verdict: LGTM / NEEDS_CHANGES / NEEDS_DISCUSSION
**Confidence:** high / medium / low *(with justification referencing calibration table)*
**Summary:** [2-3 sentences explaining the verdict]
```

---

## Verdict Consistency Rules

1. **The verdict must match your most severe finding.** If you have any ❌ Error findings, the verdict must be `NEEDS_CHANGES`. If only ⚠️ Warnings, use judgment but explain.
2. **Devil's advocate before finalizing.** Re-read all findings. For each warning, ask: "Would I be comfortable if this merged as-is?"
3. **Never approve what you can't verify.** If the fix touches platform code you can't fully reason about, say so explicitly and use `NEEDS_DISCUSSION`.
4. **LGTM means no ❌ Errors.** You can LGTM with 💡 Suggestions. You can LGTM with ⚠️ Warnings only if you've explained why they're acceptable.
5. **🚨 NEVER use `--approve` or `--request-changes` on GitHub.** Only post comments. Approval is a human decision.
6. **Output to terminal only by default.** Do not post review comments to GitHub (`gh pr review --comment`) unless explicitly asked by the user or orchestrated by another agent. This matches `pr-finalize` policy.

---

## Posting the Review

After completing your review, suggest using the `Post-CodeReview.ps1` script to format and post the comment. **Do NOT post automatically** - always let the user decide when to post.

```bash
# Save your review to a file, then suggest:
pwsh .github/scripts/Post-CodeReview.ps1 -PRNumber <PR_NUMBER> -ReviewFile /tmp/review.md -DryRun

# User can then post when ready:
pwsh .github/scripts/Post-CodeReview.ps1 -PRNumber <PR_NUMBER> -ReviewFile /tmp/review.md
```

The script wraps the review in a collapsible `<details>` section, adds PR metadata (commit SHA, title), and auto-detects the verdict for a colored status dot (🟢 Approved, 🟡 Changes Suggested, 🟠 Discussion Needed).

---

## Completion Criteria

- [ ] Full source files read (not just diffs)
- [ ] Independent assessment formed before reading PR narrative
- [ ] Prior reviews checked and critical findings reconciled (Step 4)
- [ ] CI status verified via `gh pr checks` — not assumed (Step 5)
- [ ] MAUI-specific checklist walked through for each applicable section
- [ ] Blast radius assessed for infrastructure/handler/platform changes (Step 6)
- [ ] Failure-mode probing completed with real scenarios, not softballs (Step 6)
- [ ] Findings categorized by severity (❌ / ⚠️ / 💡)
- [ ] Confidence calibrated against blast radius and evidence tables (Step 6)
- [ ] Verdict is consistent with findings AND prior review reconciliation
- [ ] Output follows the format above

---

## Lessons Learned (Regressions Caused by Skill Gaps)

### PR #34669 — Badge Feature Regression (April 2026)

**What happened:** 1141-line PR adding ToolbarItem badge support was reviewed three times. Pass 1 correctly found a critical WeakReference bug. Pass 3 contradicted Pass 1 — praised the same buggy code as "robust", gave `LGTM` with "Confidence: high", and falsely stated "No CI failures detected. Clean build." The PR was merged and caused **100% UITest failure** (app crash on startup, all platforms). Required revert PR #34984.

**Five skill gaps that caused this:**
1. **No CI verification** — said "clean build" without running `gh pr checks` (CI was actually failing)
2. **No prior review reconciliation** — silently contradicted its own earlier critical finding
3. **No blast radius assessment** — didn't realize toolbar changes affected ALL toolbars at startup
4. **Superficial failure-mode probing** — asked "Should BadgeText be int?" instead of "What if this code runs on pages without badges?"
5. **Overconfidence** — "Confidence: high" on a 1141-line, 26-file platform infrastructure change

**These gaps led to:** Phase 4 reconciliation requirement, Step 5 hard gate, blast radius assessment, failure-mode probing, and confidence calibration rules.
