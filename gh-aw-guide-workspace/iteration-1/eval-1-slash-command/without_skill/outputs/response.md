# Setting Up a `/review` Slash Command gh-aw Workflow

## Overview

You want a GitHub Agentic Workflow (gh-aw) that:
1. Triggers when someone comments `/review` on a PR
2. Reviews the PR code
3. Posts a comment with review results
4. Adds a reaction to the triggering comment

gh-aw has **built-in features** for all three of these requirements. The key insight is: **do not manually implement slash command parsing, emoji reactions, or status comments** — gh-aw provides native primitives for each.

---

## The Workflow Source File (`.md`)

Create `.github/workflows/copilot-review-pr.md`:

```markdown
---
description: Reviews PR code for correctness, safety, and best practices when triggered by /review command
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
  reaction: eyes

labels: ["pr-review"]

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
    run-started: "🔍 Reviewing PR… [{workflow_name}]({run_url})"
    run-success: "✅ Review complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Review failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "review-pr-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
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

  - name: Fetch PR diff and metadata for agent context
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json title,body,labels,baseRefName,headRefName,files > pr-metadata.json
      gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" > pr-diff.patch || true
      gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only > changed-files.txt || true
      echo "✅ PR metadata and diff fetched for PR #$PR_NUMBER"

  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Review PR

You are a code reviewer for this repository.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.issue.number || inputs.pr_number }}

The PR metadata has been saved to `pr-metadata.json`, the diff to `pr-diff.patch`,
and the list of changed files to `changed-files.txt`.

## Pre-flight check

Verify the PR data files exist:

```bash
test -f pr-metadata.json && test -f changed-files.txt
```

If missing, post a comment explaining the issue and stop.

## Instructions

1. Read `pr-metadata.json` to understand the PR's purpose (title, description, labels)
2. Read `changed-files.txt` to see which files changed
3. Read `pr-diff.patch` to analyze the actual code changes
4. For critical files, read the full file from disk (the PR branch is checked out)
5. Evaluate the changes for:
   - **Correctness**: Logic errors, off-by-one, null safety
   - **Security**: Injection, secrets, unsafe operations
   - **Performance**: Unnecessary allocations, N+1 queries, blocking calls
   - **Maintainability**: Naming, duplication, complexity
   - **Tests**: Whether tests cover the changes adequately

## When no action is needed

If the PR contains no reviewable code changes (e.g., docs-only, CI config only), call the `noop` tool:

```json
{"noop": {"message": "No action needed: PR contains no reviewable code changes"}}
```

## Posting Results

Post results using `add_comment` with `item_number` set to the PR number.

Use this format:

```markdown
## 🔍 Code Review

**Summary**: [1-2 sentence summary of the review]

### Findings

[List findings by severity: 🔴 Critical → 🟡 Suggestions → 🟢 Positive observations]

### Verdict

[✅ Looks good | ⚠️ Minor concerns | ❌ Changes requested]

> 👍 / 👎 — Was this review helpful? React to let us know!

<details>
<summary>📋 Detailed Analysis</summary>

[Detailed file-by-file analysis]

</details>
```
```

---

## Key Design Decisions Explained

### 1. Use `slash_command:` — NOT manual `issue_comment` parsing

**Anti-pattern** (❌ don't do this):
```yaml
on:
  issue_comment:
    types: [created]
# Then in steps:
#   if: startsWith(github.event.comment.body, '/review')
```

**Correct approach** (✅):
```yaml
on:
  slash_command:
    name: review
    events: [pull_request_comment]
```

**Why**: The `slash_command:` trigger is a built-in gh-aw feature that compiles down to `issue_comment` internally but provides:
- Automatic command matching (no manual `startsWith` check needed)
- Input sanitization
- Platform-managed comment handling
- Cleaner frontmatter

The `events: [pull_request_comment]` restricts triggering to comments on PRs specifically (not issues).

### 2. Use `reaction:` — NOT manual emoji API calls

**Anti-pattern** (❌):
```yaml
steps:
  - name: Add reaction
    run: |
      gh api repos/$GITHUB_REPOSITORY/issues/comments/$COMMENT_ID/reactions \
        -f content=eyes
```

**Correct approach** (✅):
```yaml
on:
  reaction: eyes
```

**Why**: The `reaction:` field under `on:` is a built-in gh-aw feature. It automatically adds the specified emoji reaction (👀 in this case) to the triggering comment when the workflow starts. No manual API call needed. This also runs with the correct permissions automatically.

Available reaction values: `+1`, `-1`, `laugh`, `confused`, `heart`, `hooray`, `rocket`, `eyes`.

### 3. Use `safe-outputs: add-comment:` with `hide-older-comments: true`

```yaml
safe-outputs:
  add-comment:
    max: 1
    target: "*"
    hide-older-comments: true
```

- **`max: 1`**: Limits the agent to posting at most 1 comment (prevents runaway agents from spamming)
- **`target: "*"`**: Required for `workflow_dispatch` triggers which don't have an implicit PR context. The `*` means the agent specifies the target item number dynamically.
- **`hide-older-comments: true`**: Automatically collapses previous review comments from this workflow when a new one is posted. This prevents stale reviews from cluttering the PR. Without this, you'd need to manually edit or delete old comments.

### 4. Use `messages:` for status comments — NOT manual posting

```yaml
messages:
  footer: "> 🔍 *Review by [{workflow_name}]({run_url})*"
  run-started: "🔍 Reviewing PR… [{workflow_name}]({run_url})"
  run-success: "✅ Review complete! [{workflow_name}]({run_url})"
  run-failure: "❌ Review failed. [{workflow_name}]({run_url}) {status}"
```

**Why**: gh-aw automatically posts these status messages at the appropriate workflow lifecycle points. The `{workflow_name}` and `{run_url}` are platform-provided template variables. This replaces any need for manual "workflow started" or "workflow completed" comments.

### 5. Always include `workflow_dispatch` as a secondary trigger

```yaml
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
```

**Why**: `workflow_dispatch` provides a manual fallback for:
- Testing the workflow without commenting on a PR
- Re-running a review when the slash command has issues
- Triggering reviews from the Actions tab in the GitHub UI

### 6. The `if:` condition filters event types

```yaml
if: >-
  github.event_name == 'issue_comment' ||
  github.event_name == 'workflow_dispatch'
```

**Why**: `slash_command:` compiles to `issue_comment` internally. This condition ensures the workflow only runs for the expected trigger types. Without it, if you later add a `pull_request` trigger, the condition prevents unintended runs.

### 7. The `steps:` section runs BEFORE the agent (with credentials)

```yaml
steps:
  - name: Fetch PR diff and metadata for agent context
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      gh pr view "$PR_NUMBER" ...
```

**Critical architecture point**: User-defined `steps:` run *before* the agent starts, *outside* the sandbox, with full `GITHUB_TOKEN` access. The agent itself runs *inside* a sandboxed container where `GITHUB_TOKEN` and `gh` CLI credentials are scrubbed.

This means:
- ✅ **DO** fetch PR data, metadata, and diffs in `steps:` (needs API access)
- ✅ **DO** run trusted scripts from the base branch in `steps:`
- ❌ **DO NOT** execute PR code or workspace scripts after checking out a fork PR in `steps:`
- ❌ **DO NOT** rely on `gh` CLI inside the agent prompt — it won't work

### 8. Concurrency prevents duplicate reviews

```yaml
concurrency:
  group: "review-pr-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true
```

**Why**: If someone comments `/review` twice quickly, the first run is cancelled. The group key includes all possible PR number sources so that concurrent reviews of *different* PRs can run in parallel while reviews of the *same* PR are serialized.

### 9. The `Checkout-GhAwPr.ps1` step for `workflow_dispatch`

```yaml
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

**Why**: For `slash_command` triggers, the platform's `checkout_pr_branch.cjs` automatically checks out the PR branch *after* all user steps. But for `workflow_dispatch`, the platform skips checkout entirely — so this script handles it manually. It also:
- Verifies PR author has write access
- Rejects fork PRs (security)
- Restores `.github/skills/` and `.github/instructions/` from the base branch (trusted infra)

---

## Compilation & Deployment

After creating the `.md` file:

```bash
# Compile the workflow (generates .lock.yml)
gh aw compile .github/workflows/copilot-review-pr.md

# Commit BOTH files
git add .github/workflows/copilot-review-pr.md .github/workflows/copilot-review-pr.lock.yml
git commit -m "Add /review slash command gh-aw workflow"
```

**Important**: The `.lock.yml` is auto-generated and must never be edited manually. Always commit it alongside the `.md` source.

**Note on slash commands**: The `/review` command won't work until the workflow is merged to the default branch (`main`). This is a platform limitation — `issue_comment`-based triggers require the workflow to exist on the default branch.

---

## Summary of Built-in Features Used (vs. Manual Reimplementation)

| Feature | Built-in gh-aw | Manual Alternative (❌ avoid) |
|---------|---------------|-------------------------------|
| Slash command parsing | `slash_command: name: review` | `issue_comment` + `startsWith(body, '/review')` |
| Emoji reaction on trigger | `reaction: eyes` | Manual `gh api` call to reactions endpoint |
| Status comments | `messages: run-started/run-success/run-failure` | Manual comment posting in steps |
| Collapse old comments | `hide-older-comments: true` | Manual comment editing/deletion |
| Comment posting | `safe-outputs: add-comment: max: 1` | Manual `gh pr comment` in agent |

Each built-in feature is **preferred** because it's battle-tested, handles edge cases (permissions, fork PRs, rate limits), and reduces workflow code.

---

## Fork PR Considerations

For the `/review` slash command on fork PRs:
- The platform's `checkout_pr_branch.cjs` checks out the fork branch **after** all user steps
- This means fork PRs can supply their own `.github/skills/` and `.github/instructions/`
- **Mitigation**: The agent is sandboxed (no credentials), limited to 1 comment via `safe-outputs`, and the prompt includes a pre-flight check
- The `Checkout-GhAwPr.ps1` script (used for `workflow_dispatch`) rejects fork PRs entirely

If you want to **allow fork PRs** on `pull_request` triggers (not applicable here since we use `slash_command`), you'd add `forks: ["*"]` to the trigger config.
