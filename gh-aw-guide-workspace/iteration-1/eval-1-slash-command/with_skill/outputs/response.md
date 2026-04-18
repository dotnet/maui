# Setting Up a `/review` Slash Command gh-aw Workflow

## TL;DR

Use the built-in `slash_command:` trigger, `reaction:` frontmatter field, and `add-comment:` safe output. Do **not** manually parse `issue_comment` events or add emoji reactions yourself — gh-aw has native support for all three of these features.

---

## Key Principle: Use Built-in gh-aw Features First

Before writing any manual implementation, the skill's **🚨 "Before You Build"** section (SKILL.md lines 32–63) is explicit:

> **CRITICAL RULE:** Before implementing any trigger, output, scheduling, or interaction mechanism in a gh-aw workflow, check whether gh-aw has a built-in feature that does it. gh-aw extends GitHub Actions with many convenience features — manually reimplementing them is always worse (more code, more bugs, missing platform integration like emoji reactions, sanitized inputs, and noise reduction).

Three items from the anti-patterns table directly apply to your request:

| What you need | Anti-pattern (manual) | Built-in to use |
|---|---|---|
| Trigger on `/review` comment | `issue_comment` + `startsWith(comment.body, '/review')` | `slash_command:` trigger ([Command Triggers docs](https://github.github.com/gh-aw/reference/command-triggers/)) |
| Emoji reaction on the triggering comment | Manual GitHub API call to add reaction | `reaction:` field under `on:` ([Frontmatter docs](https://github.github.com/gh-aw/reference/frontmatter/)) |
| Post a comment with results | (Already needs safe-outputs) | `add-comment:` safe output ([Safe Outputs docs](https://github.github.com/gh-aw/reference/safe-outputs/)) |

*(Anti-patterns table: SKILL.md lines 42–62)*

---

## Complete Workflow Example

Create the file `.github/workflows/pr-review.md`:

```yaml
---
description: Reviews PR code when someone comments /review on a pull request
on:
  slash_command:
    name: review
    events: [pull_request_comment]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to review'
        required: true
        type: number

# slash_command compiles to issue_comment internally (platform handles command
# matching). workflow_dispatch is always allowed through.
if: >-
  github.event_name == 'issue_comment' ||
  github.event_name == 'workflow_dispatch'

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-sonnet-4.6

safe-outputs:
  add-comment:
    max: 1
    target: "*"
    hide-older-comments: true
  noop:
    report-as-issue: false
  messages:
    footer: "> 🔍 *Review by [{workflow_name}]({run_url})*"
    run-started: "🔍 Reviewing this PR… [{workflow_name}]({run_url})"
    run-success: "✅ Review complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Review failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "pr-review-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 20

steps:
  - name: Gate — verify this is an open PR
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      if ! STATE=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json state --jq .state 2>&1); then
        echo "❌ Failed to fetch PR #$PR_NUMBER state: $STATE"
        exit 1
      fi
      if [ "$STATE" != "OPEN" ]; then
        echo "⏭️ PR #$PR_NUMBER is $STATE — skipping review."
        exit 1
      fi

  - name: Fetch PR metadata for agent
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json title,body,labels,baseRefName,headRefName > pr-metadata.json
      gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only > changed-files.txt
      echo "✅ Fetched metadata and changed file list for PR #$PR_NUMBER"

  # For workflow_dispatch, the platform skips checkout entirely — this step
  # gets the PR code onto disk and restores trusted infra from main.
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# PR Code Review

You are a code reviewer for the ${{ github.repository }} repository.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.issue.number || inputs.pr_number }}

The PR branch has been checked out for you. All files from the PR are available locally.

## Pre-flight check

Before starting, verify that essential skill files exist:

```bash
test -f .github/skills/code-review/SKILL.md
```

If the file is **missing**, the fork PR branch is likely not rebased on the latest `main`. Post a comment using `add_comment`:

```markdown
## 🔍 PR Code Review

❌ **Cannot review**: this PR's branch does not include the code-review skill (`.github/skills/code-review/SKILL.md` is missing).

**Fix**: rebase your fork on the latest `main` branch and push again.
```

Then stop — do not proceed with the review.

## Instructions

1. Read the PR metadata from `pr-metadata.json` and the changed file list from `changed-files.txt`
2. Use `gh pr diff` (via MCP tools) or read the local files to understand the changes
3. Review the code for correctness, safety, performance, and consistency
4. Post your review as a single comment using `add_comment` with `item_number` set to the PR number

## Output Format

Post a review comment in this structure:

```markdown
## 🔍 PR Code Review

**Overall Assessment:** [✅ Looks good | ⚠️ Minor concerns | ❌ Issues found]

[2-3 sentence summary]

> 👍 / 👎 — Was this review helpful? React to let us know!

<details>
<summary>📋 Expand Full Review</summary>

### Findings

[Detailed review organized by file or topic]

### Recommendations

[Actionable suggestions]

</details>
```
```

After creating the `.md` file, **compile it**:

```bash
gh aw compile .github/workflows/pr-review.md
```

This generates `.github/workflows/pr-review.lock.yml` and updates `.github/aw/actions-lock.json`. **Always commit both the `.md` source and the compiled `.lock.yml` together.** *(SKILL.md lines 17–28)*

---

## Detailed Explanation of Each Section

### 1. `slash_command:` Trigger (Instead of Manual `issue_comment` Parsing)

```yaml
on:
  slash_command:
    name: review
    events: [pull_request_comment]
```

The `slash_command:` trigger is a gh-aw built-in that compiles to `issue_comment` internally but gives you:

- **Automatic command matching** — the platform handles matching `/review` in the comment body; you never write `startsWith()` logic yourself.
- **Automatic emoji reaction** — when someone types `/review`, gh-aw automatically adds a 👀 (or configured) reaction to the triggering comment, signaling that the workflow was triggered. This is the `reaction:` feature referenced in SKILL.md line 46. You get this for free with `slash_command:` — no manual API call needed.
- **Sanitized input** — any arguments after `/review` are sanitized by the platform before reaching the agent.
- **Noise reduction** — only matches the exact command, unlike raw `issue_comment` which fires on every comment.

*(Anti-pattern: SKILL.md line 44: `issue_comment` + `startsWith(comment.body, '/cmd')` → use `slash_command:` trigger)*

**Important limitation**: The `slash_command` trigger compiles to `issue_comment`, which requires the workflow to be on the **default branch** (`main`) to work. You must merge the workflow to `main` before `/review` commands will trigger. *(architecture.md line 261; SKILL.md troubleshooting)*

### 2. Reaction on the Triggering Comment

The `reaction:` feature is **automatically included** when you use `slash_command:`. The platform adds an emoji reaction to the triggering comment when the workflow starts, giving the user immediate visual feedback that their `/review` command was received.

If you wanted to customize the reaction emoji or add one to a non-slash-command trigger, you would add `reaction:` under `on:` in the frontmatter:

```yaml
on:
  slash_command:
    name: review
    events: [pull_request_comment]
  reaction: eyes   # 👀 reaction (this is already the default for slash_command)
```

*(Anti-pattern: SKILL.md line 46: manual emoji reaction → use `reaction:` field under `on:`)*

**Do NOT** write manual code to add reactions via the GitHub API — the platform handles this for you.

### 3. `add-comment:` Safe Output (Posting Review Results)

```yaml
safe-outputs:
  add-comment:
    max: 1
    target: "*"
    hide-older-comments: true
```

Key options explained:

| Option | Purpose | Why |
|--------|---------|-----|
| `max: 1` | Limit to one comment per run | Prevents the agent from spamming multiple comments *(architecture.md line 249)* |
| `target: "*"` | Required for `workflow_dispatch` | Without a triggering PR context, the agent needs explicit permission to comment on any issue/PR *(SKILL.md line 91; architecture.md line 250)* |
| `hide-older-comments: true` | Collapse previous review comments from this workflow | Keeps the PR clean when re-running `/review` multiple times *(anti-pattern: SKILL.md line 51)* |

The `messages:` block provides automatic status comments:
- `run-started`: Posted when the workflow begins (replaces manual "workflow started" comments)
- `run-success`/`run-failure`: Posted on completion

*(Anti-pattern: SKILL.md line 47: manual status comments → use `status-comment: true` or `messages:` block)*

### 4. `workflow_dispatch` as a Manual Fallback

```yaml
workflow_dispatch:
  inputs:
    pr_number:
      description: 'PR number to review'
      required: true
      type: number
```

Always include `workflow_dispatch` as a secondary trigger. This allows:
- Manual triggering from the Actions tab for debugging
- Running reviews on PRs where commenting `/review` isn't possible (e.g., locked PRs)

For `workflow_dispatch`, the platform's `checkout_pr_branch.cjs` is **skipped** (architecture.md line 36), so you need the `Checkout-GhAwPr.ps1` step to get the PR code:

```yaml
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

*(Safe pattern: architecture.md lines 163–187)*

### 5. Concurrency Group

```yaml
concurrency:
  group: "pr-review-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true
```

Include **all trigger-specific PR number sources** in the concurrency group to prevent duplicate runs on the same PR. The `github.run_id` fallback handles edge cases where neither source has a value. *(SKILL.md lines 97–101)*

### 6. Pre-Agent Data Preparation (`steps:`)

The `steps:` section runs **before the agent**, in a trusted context with `GITHUB_TOKEN` access. The agent container has credentials scrubbed (architecture.md lines 22–30), so any GitHub API calls must happen here.

```yaml
steps:
  - name: Fetch PR metadata for agent
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      gh pr view "$PR_NUMBER" --json title,body,labels > pr-metadata.json
      gh pr diff "$PR_NUMBER" --name-only > changed-files.txt
```

The agent can then read `pr-metadata.json` and `changed-files.txt` from disk. *(SKILL.md lines 68–81: "Pre-Agent Data Prep")*

### 7. Security Considerations

**The agent is sandboxed** — it has no `GITHUB_TOKEN`, no `gh` CLI credentials, and no git credentials (architecture.md line 27). It can only affect the outside world through safe outputs.

Key rules for workflow authors (architecture.md lines 129–137):

- ✅ **DO** treat PR contents as passive data (read, analyze, diff)
- ✅ **DO** run data-gathering scripts in `steps:` (pre-agent, trusted context)
- ❌ **DO NOT** run `dotnet build` or any build command on untrusted PR code inside the agent
- ❌ **DO NOT** set `roles: all` on workflows that process PR content

Since this is a review workflow (read-only analysis), the security posture is straightforward — the agent reads code but never executes it.

### 8. Fork PR Behavior

For `slash_command` (which compiles to `issue_comment`):
- **Same-repo PRs**: Platform's `checkout_pr_branch.cjs` handles checkout. The platform automatically restores `.github/` and `.agents/` from the base branch after checkout (architecture.md lines 149–150, resolved in gh-aw#23769).
- **Fork PRs**: Same behavior — platform restores `.github/` from base branch artifact, preventing fork from injecting modified skills/instructions (architecture.md line 161).

The pre-flight check in the agent prompt catches the edge case where a fork hasn't rebased on `main` and the skill file is missing entirely.

*(Fork handling: architecture.md lines 140–187)*

---

## What NOT to Do

### ❌ Don't manually parse `issue_comment` events

```yaml
# ❌ ANTI-PATTERN (SKILL.md line 44)
on:
  issue_comment:
    types: [created]

# Then in steps:
- name: Check if review command
  if: startsWith(github.event.comment.body, '/review')
  run: echo "Review requested"
```

This reimplements what `slash_command:` does natively, but without command sanitization, automatic reactions, or platform-managed matching.

### ❌ Don't manually add emoji reactions

```yaml
# ❌ ANTI-PATTERN (SKILL.md line 46)
steps:
  - name: Add reaction to comment
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      gh api repos/${{ github.repository }}/issues/comments/${{ github.event.comment.id }}/reactions \
        -f content=eyes
```

The `slash_command:` trigger handles this automatically via the built-in `reaction:` mechanism.

### ❌ Don't post manual status comments

```yaml
# ❌ ANTI-PATTERN (SKILL.md line 47)
steps:
  - name: Post "started" comment
    run: gh pr comment "$PR_NUMBER" --body "🔍 Review started..."
```

Use the `messages:` block under `safe-outputs:` instead — it's managed by the platform and includes run URLs automatically.

---

## Compilation and Deployment Checklist

1. **Create** the `.md` workflow file at `.github/workflows/pr-review.md`
2. **Compile**: `gh aw compile .github/workflows/pr-review.md`
3. **Commit both files**: the `.md` source AND the generated `.lock.yml`
4. **Merge to `main`**: The `/review` slash command won't work until the workflow is on the default branch *(architecture.md line 261)*
5. **Test**: Comment `/review` on any open PR to verify the workflow triggers

---

## Summary of Skill Sections Referenced

| Topic | Location |
|-------|----------|
| Built-in features rule | SKILL.md § "Before You Build" (lines 32–63) |
| Anti-patterns table (slash_command, reaction, status-comment) | SKILL.md lines 42–62 |
| Pre-agent data prep (`steps:` pattern) | SKILL.md lines 68–81 |
| Safe outputs (`add-comment:`) | SKILL.md lines 84–91, architecture.md lines 247–250 |
| Concurrency groups | SKILL.md lines 97–101 |
| Execution model & credential availability | architecture.md lines 1–30 |
| Security rules for authors | architecture.md lines 129–137 |
| Fork PR handling & platform restore | architecture.md lines 140–187 |
| `slash_command` compiles to `issue_comment` | architecture.md line 161 |
| Workflow must be on `main` for slash commands | architecture.md line 261 |
| Compilation instructions | SKILL.md lines 17–28 |
