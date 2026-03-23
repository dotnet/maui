---
description: Runs AI pre-flight analysis on PRs — gathers context from issues, review comments, and changed files, then posts a structured summary comment
on:
  pull_request:
    types: [opened, synchronize]
    paths:
      - '.github/workflows/pr-preflight*'
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to run pre-flight on'
        required: true
        type: number

if: >-
  github.event_name == 'pull_request' ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/preflight'))

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-sonnet-4

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  noop:
  messages:
    footer: "> 🤖 *Pre-flight by [{workflow_name}]({run_url})*"
    run-started: "🛫 [{workflow_name}]({run_url}) is gathering context for pre-flight analysis..."
    run-success: "✅ [{workflow_name}]({run_url}) — Pre-flight complete!"
    run-failure: "❌ [{workflow_name}]({run_url}) — Pre-flight {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "pr-preflight-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 10

steps:
  - name: Gather PR context
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: |
      set -euo pipefail
      REPO="$GITHUB_REPOSITORY"
      OUT_DIR="/tmp/gh-aw/preflight-context"
      mkdir -p "$OUT_DIR"

      echo "📋 Gathering context for PR #$PR_NUMBER..."

      # 1. PR metadata
      gh pr view "$PR_NUMBER" --repo "$REPO" \
        --json number,title,body,author,labels,baseRefName,headRefName,state,url,files,comments,reviewComments \
        > "$OUT_DIR/pr.json"
      echo "✅ PR metadata saved"

      # 2. Changed files with diff stats
      gh pr diff "$PR_NUMBER" --repo "$REPO" --name-only > "$OUT_DIR/changed-files.txt" 2>/dev/null || true
      gh api "repos/$REPO/pulls/$PR_NUMBER/files" --paginate \
        --jq '.[] | "\(.status)\t\(.additions)\t\(.deletions)\t\(.filename)"' \
        > "$OUT_DIR/file-stats.tsv"
      echo "✅ File list saved ($(wc -l < "$OUT_DIR/file-stats.tsv") files)"

      # 3. Find linked issues from PR body
      BODY=$(gh pr view "$PR_NUMBER" --repo "$REPO" --json body --jq '.body // ""')
      ISSUES=$(echo "$BODY" | grep -oE '(Fixes|Closes|Resolves|Fix|Close|Resolve)\s+#[0-9]+' | grep -oE '#[0-9]+' | tr -d '#' | sort -u || true)

      if [ -n "$ISSUES" ]; then
        for ISSUE_NUM in $ISSUES; do
          echo "  Fetching issue #$ISSUE_NUM..."
          gh issue view "$ISSUE_NUM" --repo "$REPO" \
            --json number,title,body,labels,state,comments \
            > "$OUT_DIR/issue-${ISSUE_NUM}.json" 2>/dev/null || true
        done
        echo "✅ Linked issues saved"
      else
        echo "ℹ️ No linked issues found in PR body"
      fi

      # 4. PR review comments (inline code feedback)
      gh api "repos/$REPO/pulls/$PR_NUMBER/comments" --paginate \
        --jq '.[] | "**\(.user.login)** on `\(.path):\(.line // .original_line // "?")`:\n\(.body)\n---"' \
        > "$OUT_DIR/review-comments.md" 2>/dev/null || true
      REVIEW_COUNT=$(gh api "repos/$REPO/pulls/$PR_NUMBER/comments" --jq 'length' 2>/dev/null || echo "0")
      echo "✅ Inline review comments saved ($REVIEW_COUNT)"

      # 5. Write summary index
      cat > "$OUT_DIR/index.md" <<EOF
      # Pre-flight Context for PR #$PR_NUMBER

      ## Files in this directory
      - pr.json — Full PR metadata, conversation comments, labels, files
      - file-stats.tsv — Changed files (status, additions, deletions, path)
      - changed-files.txt — File paths only
      - review-comments.md — Inline code review comments
      - issue-*.json — Linked issue data (if any)

      ## PR Number
      $PR_NUMBER
      EOF

      echo ""
      echo "📦 Context gathered in $OUT_DIR:"
      ls -la "$OUT_DIR/"
---

# AI Pre-flight Analysis

You are the Pre-flight Analyst for the dotnet/maui repository. Your job is to gather context about a Pull Request and produce a **structured summary comment** on the PR.

> **SCOPE:** Context gathering only. No code analysis. No fix opinions. No running tests.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

All PR context has been pre-fetched into `/tmp/gh-aw/preflight-context/`:
- `pr.json` — PR metadata, body, conversation comments, labels, files
- `file-stats.tsv` — Changed files (status, additions, deletions, path)
- `review-comments.md` — Inline code review comments
- `issue-*.json` — Linked issue data (body, comments, labels)

**Read these files to gather context. Do NOT use GitHub MCP tools for PR/issue data — it is all in these files.**

## Instructions

### 1. Read PR metadata

Read `/tmp/gh-aw/preflight-context/pr.json`. Extract: title, body, author, labels, base/head branches, state, changed files list and conversation comments.

### 2. Read linked issues

Read any `/tmp/gh-aw/preflight-context/issue-*.json` files. Extract: title, body, labels, comments (reproduction steps, discussion, context). If no issue files exist, note "No linked issue found."

### 3. Read PR review comments

Read `/tmp/gh-aw/preflight-context/review-comments.md` for inline code feedback. These often contain the most important technical feedback.

Also check the `comments` field in `pr.json` for conversation-level comments. Look for prior AI reviews (comments containing "Final Recommendation" with phase status tables).

### 4. Classify changed files

Categorize each file:

| Category | How to identify |
|----------|----------------|
| Implementation | `.cs`, `.xaml` not in test folders |
| UI Tests | `TestCases.HostApp` or `TestCases.Shared.Tests` |
| Device Tests | `DeviceTests` folders |
| XAML Unit Tests | `Xaml.UnitTests` |
| Unit Tests | Other `UnitTests` projects |
| Build / Config | `.csproj`, `.props`, `.targets`, YAML pipelines |
| Documentation | `.md`, `.txt`, `docs/` |

Identify platform scope from file paths:
- `Platform/Android/` or `*.Android.cs` → Android
- `Platform/iOS/` or `*.iOS.cs` → iOS (+MacCatalyst)
- `Platform/MacCatalyst/` or `*.MacCatalyst.cs` → MacCatalyst only
- `Platform/Windows/` or `*.Windows.cs` → Windows
- No platform folder → Cross-platform

### 5. Document edge cases

From issue and PR comments, extract mentions of:
- "what about…", "does this work with…", "have you considered…"
- Platform-specific concerns, performance implications, breaking-change potential

### 6. Summarize the fix approach

Based on PR description and changed files, briefly describe:
- What the PR fixes or implements
- The approach taken
- Key files modified and why

## Posting results

Call `add_comment` with `item_number` set to the PR number.

The comment **MUST** start with `<!-- AI-PREFLIGHT -->` on the very first line (used for identification on re-runs).

Use this format:

```markdown
<!-- AI-PREFLIGHT -->
## 🤖 AI Pre-flight

| Field | Value |
|-------|-------|
| **PR** | #XXXXX — {title} |
| **Issue** | #YYYYY — {title} _(or "No linked issue found")_ |
| **Author** | @{author} |
| **Base** | `{base_branch}` |
| **Platforms** | {platforms from labels or file analysis} |
| **Files** | {N} implementation, {M} test, {K} other |

<details>
<summary>📋 Issue Summary</summary>

{Concise summary: what is broken, expected vs actual behavior, reproduction steps if available.}

</details>

<details>
<summary>📁 File Classification ({total} files changed)</summary>

**Implementation Files**

| File | Status | Changes |
|------|--------|---------|
| `path/to/file.cs` | Modified | +10 / -3 |

**Test Files**

| File | Type | Status | Changes |
|------|------|--------|---------|
| `path/to/test.cs` | UI Test | Added | +50 / -0 |

**Other Files**

| File | Category | Status |
|------|----------|--------|
| `path/to/file.csproj` | Build Config | Modified |

**Platform scope:** {affected platforms}

</details>

<details>
<summary>💬 Review Comments Summary ({N} comments)</summary>

- **Technical feedback**: {key inline review points}
- **Design discussion**: {architectural or design discussions}
- **Requests**: {specific change requests from reviewers}

</details>

<details>
<summary>⚠️ Edge Cases & Considerations</summary>

- {edge case or concern 1}
- {edge case or concern 2}

</details>

<details>
<summary>🔧 Fix Approach</summary>

**What:** {what the PR fixes or implements}

**How:** {the approach taken}

**Key files:**
- `{file1}` — {what changed and why}
- `{file2}` — {what changed and why}

</details>
```

## Rules

- **Context gathering ONLY** — do NOT analyze code correctness, suggest fixes, or evaluate quality
- **Be factual** — report what you find, do not speculate
- **Preserve detail** — include error messages, version numbers, platform details from the issue
- **Keep it scannable** — use bullet points and tables, not long paragraphs
- **`<!-- AI-PREFLIGHT -->` MUST be the first line** of the comment body
- If you encounter errors, call `noop` with a message explaining what went wrong
