---
name: code-review
description: >-
  Deep code review of PR changes for correctness, safety, and MAUI conventions.
  Uses independence-first assessment (code before narrative) and delegates to the
  maui-expert-reviewer agent for per-dimension sub-agent evaluation. Triggers on:
  "review code for PR", "code review PR", "analyze code changes", "check PR code quality".
  Do NOT use for: summarizing PRs, describing what changed, general PR questions,
  running tests, or fixing code.
---

# Code Review Skill

Standalone skill that evaluates PR code changes for correctness, safety, performance, and consistency with .NET MAUI conventions. Can be invoked directly by users or by other agents/skills.

**Trigger phrases:** "review code for PR #XXXXX", "code review PR #XXXXX", "review this PR's code", "analyze code changes in PR", "check PR code quality"

**Do NOT use for:** "what does PR #XXXXX do?", "summarize PR", "describe the changes", or any informational query — just answer those directly without invoking this skill.

> **How this differs from other skills:**
> - **`pr-review`** — End-to-end PR workflow (4 phases: pre-flight, gate, try-fix, report). Use when you want the full pipeline including test verification and fix attempts.
> - **`pr-finalize`** — Verifies PR title/description match implementation + light code review. Use before merging.
> - **`code-review`** (this skill) — Deep code-only review with MAUI domain rules. Use when you want a thorough code analysis without running tests or modifying the PR.

## Core Principles

1. **Independence-first** — Form your assessment from the code BEFORE reading the PR description. This prevents anchoring on the author's framing
2. **Full-context** — Read entire source files, not just diffs. Check callers, consumers, and git history
3. **Empirical grounding** — Reference specific code, line numbers, and call sites. No vague concerns
4. **Severity calibration** — Distinguish errors from warnings from suggestions. Not everything is critical
5. **Failure-mode probing** — Challenge your own conclusions with real failure scenarios, not softballs

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

### Step 2: Delegate to Expert Reviewer

Delegate to the `maui-expert-reviewer` agent (`.github/agents/maui-expert-reviewer.md`) which runs per-dimension sub-agent evaluation. The agent's sole output is `inline-findings.json` — file:line comments in GitHub Review API format.

**After the agent finishes:**

- **If `COMMENTS_VIA_FILE=true`** (CI): Done. The pipeline calls `post-inline-review.ps1` to post findings using `GH_COMMENT_TOKEN`.
- **If `COMMENTS_VIA_FILE` is unset** (local): Post inline findings directly:
  ```bash
  COMMIT_SHA=$(gh pr view $PR_NUMBER --json headRefOid --jq .headRefOid)
  gh api repos/dotnet/maui/pulls/$PR_NUMBER/reviews \
    --method POST \
    --input <(jq -n \
      --arg sha "$COMMIT_SHA" \
      --arg body "Expert review — see inline comments." \
      --argjson comments "$(cat CustomAgentLogsTmp/PRState/$PR_NUMBER/PRAgent/inline-findings.json)" \
      '{commit_id: $sha, body: $body, event: "COMMENT", comments: [$comments[] | {path, line, body, side: "RIGHT"}]}')
  ```

### Step 3: Form Independent Assessment

Based ONLY on the code (no PR description), answer:

1. **What does this change do?** Describe the behavioral change in your own words
2. **Why might it be needed?** Infer motivation from the code
3. **Is the approach sound?** Would a simpler alternative work?
4. **What problems do you see?** Run through the agent's dimension CHECKs for matched dimensions

### Step 4: Read PR Narrative and Reconcile

Now read the PR description, linked issue, and comments. Treat these as **claims to verify**, not facts.

1. Where your assessment disagrees with the author's claims, investigate further
2. If the PR claims a bug fix, verify the root cause analysis matches the code
3. Check existing review comments to avoid duplicating feedback

#### 🚨 Prior Review Reconciliation

Check for prior reviews on the same PR — from the Copilot PR reviewer bot, other agents, or human reviewers:

```bash
gh pr view <PR_NUMBER> --json reviews --jq '.reviews[] | "Reviewer: \(.author.login) | State: \(.state)\n\(.body[0:2000])\n---"'
```

**If prior reviews flagged critical issues:**
- Verify whether each critical finding was addressed in subsequent commits
- If unresolved → verdict must be `NEEDS_CHANGES`
- If status cannot be determined → default to unresolved (caution over optimism)
- **NEVER silently drop or contradict a prior critical finding**

### Step 5: Blast Radius, Failure-Mode Probing, and Verdict

#### Blast Radius Assessment

**Required when PR modifies:** handlers, platform extensions, toolbar/navigation code, page registration, static state, `PropertyChanged` subscriptions, or startup paths.

| Question | Why It Matters |
|----------|---------------|
| Does this code run for ALL instances, or only when the new feature is used? | Feature code that runs unconditionally is the #1 cause of startup crashes |
| Does this code run at app startup or page initialization? | Static fields initialized on first access can crash the app before any test page loads |
| Are there new static/shared state fields that affect all pages/windows? | Static state survives handler disposal unless explicitly scoped |
| What happens at startup with null/default values for new properties? | New BindableProperty with null default must not cause NullRef in platform code paths |

#### Failure-Mode Probing

**Do NOT ask easy rhetorical questions.** Probe genuinely challenging failure modes:

- What happens if this code runs on items/pages that DON'T use the new feature?
- What happens during handler disconnect/reconnect (navigation, Shell tab switch)?
- What happens with null `Parent`, `Handler`, `BindingContext`, or `PlatformView`?
- Can multiple subscriptions accumulate across handler lifecycle (missing unsubscribe)?
- Does static state survive page disposal and get stale?

#### Confidence Calibration

| Blast Radius | Max Confidence |
|-------------|----------------|
| Localized change, non-startup, non-infrastructure | May be **high** |
| Platform-specific handler/UI plumbing | Max **medium** |
| Shared infrastructure, startup path, global static state | Max **low** |

**Then cap by evidence:**

| Evidence | Confidence Cap |
|----------|---------------|
| CI red or pending | Defer to `azdo-build-investigator` skill for CI analysis |
| No relevant tests run (UITests skip PR builds) | Max **low** |
| Prior critical findings unresolved | **NEEDS_CHANGES** (no LGTM) |

#### Deliver Verdict

- **`LGTM`** — Code is correct, safe, and consistent with MAUI patterns. Ready for human approval.
- **`NEEDS_CHANGES`** — Concrete issues found that should be addressed before merge.
- **`NEEDS_DISCUSSION`** — Complex tradeoffs or architectural questions that need human judgment.

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

### Prior Review Reconciliation
| Prior Critical Finding | Source | Status | Evidence |
|------------------------|--------|--------|----------|
| [finding] | [reviewer] | ✅ Fixed / ❌ Unresolved / 🔄 Obsolete | [evidence] |
*(If no prior reviews with critical findings, state "No prior critical findings found.")*

### Blast Radius Assessment
*(Required for infrastructure/handler/platform changes; omit for simple fixes)*
- Runs for all instances: [yes/no — explanation]
- Startup impact: [yes/no]
- Static/shared state: [yes/no]

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
**Confidence:** high / medium / low *(justified against calibration table)*
**Summary:** [2-3 sentences explaining the verdict]
```

---

## Verdict Consistency Rules

1. **The verdict must match your most severe finding.** If you have any ❌ Error findings, the verdict must be `NEEDS_CHANGES`. If only ⚠️ Warnings, use judgment but explain.
2. **Failure-mode probing before finalizing.** Re-read all findings. For each warning, ask: "Would I be comfortable if this merged as-is?"
3. **Never approve what you can't verify.** If the fix touches platform code you can't fully reason about, say so explicitly and use `NEEDS_DISCUSSION`.
4. **LGTM means no ❌ Errors.** You can LGTM with 💡 Suggestions. You can LGTM with ⚠️ Warnings only if you've explained why they're acceptable.
5. **Prior critical findings override.** If any prior review flagged a critical issue that remains unresolved, verdict must be `NEEDS_CHANGES` regardless of your own assessment.
6. **🚨 NEVER use `--approve` or `--request-changes` on GitHub.** Only post comments. Approval is a human decision.
7. **Write findings to disk, do not post directly.** The agent does not have the GitHub comment token. Write findings to `CustomAgentLogsTmp/PRState/{PR}/PRAgent/` — the CI pipeline or posting scripts handle GitHub interaction.

---

## Posting the Review

The agent writes findings to disk. Posting is done separately by `Review-PR.ps1`:

**Inline review comments** (preferred — findings at exact file:line):
```bash
# Preview first:
pwsh .github/scripts/post-inline-review.ps1 -PRNumber <PR_NUMBER> -DryRun

# Post when ready:
pwsh .github/scripts/post-inline-review.ps1 -PRNumber <PR_NUMBER>
```

**Wall-of-text summary** (phase content assembled into a single PR comment):
```bash
# Called by Review-PR.ps1 automatically:
pwsh .github/scripts/post-ai-summary-comment.ps1
```

In CI (`eng/pipelines/ci-copilot.yml`), `Review-PR.ps1` calls both `post-inline-review.ps1` (for inline findings) and `post-ai-summary-comment.ps1` (for the wall-of-text from `{phase}/content.md` files), using `GH_COMMENT_TOKEN`.

---

## Completion Criteria

- [ ] Full source files read (not just diffs)
- [ ] Independent assessment formed before reading PR narrative
- [ ] Prior reviews checked and critical findings reconciled (Step 4)
- [ ] MAUI-specific checklist walked through for each applicable section
- [ ] Blast radius assessed for infrastructure/handler/platform changes (Step 6)
- [ ] Failure-mode probing completed with real scenarios, not softballs (Step 6)
- [ ] Findings categorized by severity (❌ / ⚠️ / 💡)
- [ ] Confidence calibrated against blast radius and evidence tables (Step 6)
- [ ] Verdict is consistent with findings AND prior review reconciliation
- [ ] Output follows the format above
