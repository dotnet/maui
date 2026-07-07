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
   gh pr diff <PR_NUMBER> --repo dotnet/maui
   ```

2. **Read full source files** for every changed file (not just diff hunks):
   ```bash
   gh pr diff <PR_NUMBER> --repo dotnet/maui --name-only
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
  COMMIT_SHA=$(gh pr view $PR_NUMBER --repo dotnet/maui --json headRefOid --jq .headRefOid)
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

Check for prior reviews on the same PR — from the Copilot PR reviewer bot, other agents, or human reviewers. **You MUST query all THREE surfaces** — top-level review bodies, inline review comments, AND PR issue comments. Different reviewers post findings to different surfaces: review-API bots (MauiBot, Copilot bot, this skill's adversarial reviewer) post stubs like *"Expert Review — 3 findings, see inline comments"* at the top level with the actual `❌`/`⚠️`/`💡` markers in inline comments; AI Summary bots and prior-round wall-of-text summaries post to the **issue-comments** surface (which the review API does NOT return). Querying any subset silently misses findings.

```bash
# Surface 1: top-level review bodies (review-API stubs, verdicts, human reviewer prose):
gh pr view <PR_NUMBER> --repo dotnet/maui --json reviews --jq '.reviews[] | select((.body // "") != "") | "Reviewer: \(.author.login) | State: \(.state)\n\(.body)\n---"'

# Surface 2: inline review comments (where MauiBot/Copilot/this skill post ❌/⚠️/💡 findings):
gh api repos/dotnet/maui/pulls/<PR_NUMBER>/comments --paginate \
  --jq '.[] | "\(.user.login) @ \(.path):\(.line // .original_line // 0)\n\(.body)\n---"'

# Surface 3: PR issue comments (where AI Summary bots and prior round wall-of-text summaries post):
gh api repos/dotnet/maui/issues/<PR_NUMBER>/comments --paginate \
  --jq '.[] | "\(.user.login) @ \(.created_at)\n\(.body)\n---"'
```

Scan all three outputs for `❌` markers, `[major]`/`[moderate]` tags, or equivalent severity language from other reviewer formats. Do NOT slice/truncate the body fields — long bot reviews routinely exceed 10K chars and have severity markers in the tail (empirically observed on this skill's own PRs: MauiBot reviews of 26K+ chars with `❌` markers past char 10000); truncating silently drops them and causes false `LGTM`.

**If prior reviews flagged ❌ Error-level issues:**
- Verify whether each ❌ Error finding was addressed in subsequent commits
- If unresolved → verdict must be `NEEDS_CHANGES`
- If status cannot be determined → default to unresolved (caution over optimism)
- **NEVER silently drop or contradict a prior ❌ Error finding** — confirm it no longer applies to current code before dismissing

### Step 5: Check CI Status

Before delivering a verdict, **collect the required-check status for the PR**. Don't infer CI state from absence of evidence and don't rely on prior commits' status.

```bash
gh pr checks <PR_NUMBER> --repo dotnet/maui --required
```

**Exit-code semantics (read this before classifying):** `gh pr checks --required` exit codes are NOT a reliable signal on their own — `gh` overloads them. **Always inspect stdout/stderr.**
- Exit `0` is NOT a "clean pass" signal — checks marked `skipping` (e.g., `maui-pr skipping`) also exit `0`. Read the stdout rows for actual state.
- Exit `1` is **overloaded** with three cases that look similar but require different responses:
  - **(a) Failing required check** — stdout lists one or more `fail` rows.
  - **(b) Zero required checks for the branch** — stdout is empty and stderr contains the substring `checks reported` (specifically either `no checks reported on the '<branch>' branch` when the PR has zero checks of any kind, or `no required checks reported on the '<branch>' branch` when checks exist but none are required). Both shapes mean the PR has no required gates, NOT a tool failure. Route to the *Skipped, pending, or empty result* bullet below.
  - **(c) `gh` itself errored** — stdout has no check rows and stderr contains `GraphQL:`, `Could not resolve`, `HTTP 4xx/5xx`, or `error:`. Route to the tool-unavailable fallback at the bottom of this section.
- Exit `8` means required checks are pending and `gh` is reporting normally.
- Other non-zero exits (e.g., auth failure: `gh auth login`, network failure, `command not found`) DO indicate tool unavailability and should trigger the fallback at the bottom of this section.

Classify based on the stdout row content (`pass`/`fail`/`skipping`/`pending`) **and** the stderr message, not the exit code alone. If stdout has no check rows and stderr contains a `GraphQL:` / `Could not resolve` / `error:` message, treat as **tool-unavailable** (fallback). If stdout has no check rows and stderr contains `checks reported` (either spelling — see (b) above), treat as **empty result** (not a tool failure).

- **PR-caused failing check** (compile/build errors, test failures in modified code) → flag as ❌ Error and `NEEDS_CHANGES`. Surface this in the CI Status / Verdict sections; do NOT also generate per-line inline comments duplicating compiler output (the inline-comment rule in *Review Output Format* still applies).
- **Pre-existing infra flake or known issue** (cross-reference with `azdo-build-investigator` skill if uncertain) → note in summary but still cap confidence per the table in Step 6
- **Ambiguous** → invoke the `azdo-build-investigator` skill to determine root cause before finalizing
- **PR description acknowledges the failure** → note that the author has documented the dependency; the failure still caps confidence
- **Skipped, pending, or empty result** (required check listed as `skipping`/`pending`, or `gh` exits `1` with stderr containing `checks reported` — see Exit-1 case (b) — and no stdout rows) → treat CI coverage as **undetermined**. Do not interpret an empty/skipped result as a passing build. Cap confidence at **low** and **do NOT post `LGTM`** — use `NEEDS_DISCUSSION` (per Rule #6, which prohibits LGTM on pending/undetermined CI as strictly as on red CI).

**Never claim "clean build" or `LGTM` without running this step.** Apply the *tool-unavailable* fallback when `gh` cannot determine CI state — either because `gh` itself is missing/unauthenticated (`command not found`, `gh: To get started with GitHub CLI, please run: gh auth login`), or because the command returned a tool/API error instead of check rows (stderr contains `GraphQL:` / `Could not resolve` / `error:` / `HTTP 4xx/5xx` and stdout has no `pass`/`fail`/`skipping`/`pending` rows). In any of those cases, record the gap explicitly and cap verdict confidence at **low**.

### Step 6: Blast Radius, Failure-Mode Probing, and Verdict

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

**Then cap by evidence.** The cap and the action required are separate columns — a cap alone is not a verdict, and the action does not change the cap:

| Evidence | Confidence Cap | Required Action |
|----------|----------------|-----------------|
| CI red or pending | Max **low** | Invoke `azdo-build-investigator` skill to classify failures. Per Rule #6, do not post `LGTM` unless failures are confirmed PR-unrelated. |
| No relevant tests run (UITests skip PR builds) | Max **low** | Note the coverage gap in the CI Status section. |
| Prior ❌ Error findings unresolved | n/a — overrides cap | Per Rule #5, verdict is **NEEDS_CHANGES** regardless of own assessment. |

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
- **Don't duplicate CI output as inline comments.** Skip line-by-line compiler errors and linter findings in inline `file:line` comments — CI already surfaces those. CI-detected failures must still drive the verdict and appear in the CI Status / Verdict sections per Step 5; this rule only governs the *inline-comment* surface.
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
| Prior ❌ Error Finding | Source | Status | Evidence |
|------------------------|--------|--------|----------|
| [finding] | [reviewer] | ✅ Fixed / ❌ Unresolved / 🔄 Obsolete | [evidence] |
*(If no prior reviews with ❌ Error findings, state "No prior ❌ Error findings found.")*

### Blast Radius Assessment
*(Required for infrastructure/handler/platform changes; omit for simple fixes)*
- Runs for all instances: [yes/no — explanation]
- Startup impact: [yes/no]
- Static/shared state: [yes/no]

### CI Status
*(Required — record what `gh pr checks --required` returned per Step 5)*
- Required-check result: [pass / fail / pending / skipping / no required checks]
- Classification: [PR-caused failure ❌ / pre-existing flake / undetermined / PR-acknowledged]
- Action taken: [none / invoked `azdo-build-investigator` / capped confidence]

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
5. **Prior ❌ Error findings override.** If any prior review flagged an ❌ Error-level issue (using this skill's severity taxonomy) that remains unresolved in the current code, verdict must be `NEEDS_CHANGES` regardless of your own assessment. Confirm the finding still applies to the current diff before applying the override.
6. **Never LGTM if CI is red, pending, or undetermined.** If required CI checks are failing, invoke `azdo-build-investigator` to determine whether failures are PR-caused. Do not post `LGTM` until CI passes or failures are confirmed PR-unrelated. If required checks are pending, skipping, or absent, use `NEEDS_DISCUSSION` — code review alone does not warrant LGTM when CI hasn't run. Even when failures are confirmed PR-unrelated, the Step 6 confidence cap still applies (max **low**).
7. **🚨 NEVER use `--approve` or `--request-changes` on GitHub.** Only post comments. Approval is a human decision.
8. **Findings handling depends on environment.** In CI (`COMMENTS_VIA_FILE=true`), the code-review agent does NOT have the GitHub comment token; the pipeline posts on its behalf. The `maui-expert-reviewer` sub-agent invoked in Step 2 is the *sole* producer of `CustomAgentLogsTmp/PRState/{PR}/PRAgent/inline-findings.json` (structured file:line JSON in GitHub Review API shape), which `Review-PR.ps1` posts via `post-inline-review.ps1`; the code-review skill's wall-of-text summary is posted separately by `post-ai-summary-comment.ps1`. **Do NOT have the wall-of-text-producing code-review agent emit, overwrite, or merge into `inline-findings.json` itself** — overwriting that file with prose findings will corrupt its JSON schema and break `post-inline-review.ps1`. (When the orchestrator pipeline says "write inline findings to `inline-findings.json`", it means: ensure the Step 2 expert reviewer ran and produced that file — not that the wall-of-text agent should author the JSON directly.) In local invocation (no `COMMENTS_VIA_FILE`), the agent may post directly using its own `gh` credentials per the Step 2 `gh api ... reviews --method POST` command. In either mode, Rule #7 still applies: never `--approve` or `--request-changes`.

---

## Posting the Review

In CI mode (`COMMENTS_VIA_FILE=true`) the agent writes findings to disk and posting is done separately by `Review-PR.ps1`. In local invocation (no `COMMENTS_VIA_FILE`) the agent may post directly per Rule #8 / Step 2.

**Inline review comments** (preferred — findings at exact file:line):
```bash
# Preview first:
pwsh .github/scripts/post-inline-review.ps1 -PRNumber <PR_NUMBER> -DryRun

# Post when ready:
pwsh .github/scripts/post-inline-review.ps1 -PRNumber <PR_NUMBER>
```

**Wall-of-text summary** (phase content assembled into a PR review body):
```bash
# Called by Review-PR.ps1 automatically:
pwsh .github/scripts/post-ai-summary-comment.ps1
```

In CI (`eng/pipelines/ci-copilot.yml`), `Review-PR.ps1` calls both `post-inline-review.ps1` (for inline findings) and `post-ai-summary-comment.ps1` (for the wall-of-text from `{phase}/content.md` files), using `GH_COMMENT_TOKEN`. The trusted posting script may submit `APPROVE` or `REQUEST_CHANGES` from the final recommendation; the agent itself must not run review commands directly.

---

## Completion Criteria

- [ ] Full source files read (not just diffs)
- [ ] Independent assessment formed before reading PR narrative
- [ ] Prior reviews checked and ❌ Error findings reconciled (Step 4)
- [ ] MAUI-specific checklist walked through for each applicable section
- [ ] CI status collected via `gh pr checks --required` and classified (Step 5)
- [ ] Blast radius assessed for infrastructure/handler/platform changes (Step 6)
- [ ] Failure-mode probing completed with real scenarios, not softballs (Step 6)
- [ ] Findings categorized by severity (❌ / ⚠️ / 💡)
- [ ] Confidence calibrated against blast radius and evidence tables (Step 6)
- [ ] Verdict is consistent with findings AND prior review reconciliation
- [ ] Output follows the format above
