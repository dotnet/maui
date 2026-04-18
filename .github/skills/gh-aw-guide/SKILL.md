---
name: gh-aw-guide
description: >-
  Comprehensive guide for building and maintaining GitHub Agentic Workflows (gh-aw).
  Covers architecture, security boundaries, fork handling, safe outputs, anti-patterns,
  compilation, and troubleshooting. Use when creating or editing gh-aw workflow .md files,
  writing safe-outputs configurations, configuring fork PR handling, setting up integrity
  filtering, debugging "why doesn't my workflow trigger", or any task involving
  .github/workflows/*.md or .lock.yml files. Also use when asked about gh-aw features,
  slash commands, pre-agent-steps, protected files, or agentic workflow security.
---

# gh-aw (GitHub Agentic Workflows) Guide

This skill provides the complete reference for building, securing, and maintaining GitHub Agentic Workflows in this repository. It covers the gh-aw platform's architecture, security model, and all available features.

## Quick Start

gh-aw workflows are authored as `.md` files with YAML frontmatter, compiled to `.lock.yml` via `gh aw compile`. The lock file is auto-generated — **never edit it manually**.

```bash
# Compile after every change to the .md source
gh aw compile .github/workflows/<name>.md

# This updates:
# - .github/workflows/<name>.lock.yml (auto-generated)
# - .github/aw/actions-lock.json
```

**Always commit the compiled lock file alongside the source `.md`.**

## 🚨 Before You Build: Prefer Built-in gh-aw Features

**CRITICAL RULE:** Before implementing any trigger, output, scheduling, or interaction mechanism in a gh-aw workflow, check whether gh-aw has a built-in feature that does it. gh-aw extends GitHub Actions with many convenience features — manually reimplementing them is always worse (more code, more bugs, missing platform integration like emoji reactions, sanitized inputs, and noise reduction).

### Step 1: Check the anti-patterns table below
### Step 2: If not listed, check the [triggers reference](https://github.github.com/gh-aw/reference/triggers/), [frontmatter reference](https://github.github.com/gh-aw/reference/frontmatter/), and [safe-outputs reference](https://github.github.com/gh-aw/reference/safe-outputs/)
### Step 3: If a built-in exists, use it. If not, proceed with manual implementation.

### Anti-Patterns: Manual Reimplementations to Avoid

| If you're about to implement... | Use this built-in instead | Docs |
|---------------------------------|--------------------------|------|
| `issue_comment` + `startsWith(comment.body, '/cmd')` | `slash_command:` trigger | [Command Triggers](https://github.github.com/gh-aw/reference/command-triggers/) |
| Manual emoji reaction on triggering comment | `reaction:` field under `on:` | [Frontmatter](https://github.github.com/gh-aw/reference/frontmatter/) |
| Posting "workflow started/completed" status comments | `status-comment: true` under `on:` | [Frontmatter](https://github.github.com/gh-aw/reference/frontmatter/) |
| Fixed cron schedule (`0 9 * * 1`) for non-critical timing | `schedule: weekly on monday around 9:00` (fuzzy) | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual `if:` to skip bot-authored PRs | `skip-bots:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual `if:` to skip by author role | `skip-roles:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual label check + removal for one-shot commands | `label_command:` trigger | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Editing old comments to collapse them | `hide-older-comments: true` on `add-comment:` | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |
| Creating no-op report issues | `noop: report-as-issue: false` | [Safe Outputs / Monitoring](https://github.github.com/gh-aw/patterns/monitoring/) |
| Auto-closing older issues from same workflow | `close-older-issues: true` on `create-issue:` | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |
| Disabling workflow after a date | `stop-after:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manual approval gating | `manual-approval:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Search-based skip logic in `steps:` | `skip-if-match:` / `skip-if-no-match:` under `on:` | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Locking issues to prevent concurrent edits | `lock-for-agent: true` under trigger | [Triggers](https://github.github.com/gh-aw/reference/triggers/) |
| Manually hiding agent comments | `hide-comment:` safe output | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |
| Custom post-processing jobs for agent output | `safe-outputs.jobs:` custom jobs with MCP tool access | [Custom Safe Outputs](https://github.github.com/gh-aw/reference/custom-safe-outputs/) |
| Wrapping GitHub Actions as agent-callable tools | `safe-outputs.actions:` action wrappers | [Custom Safe Outputs](https://github.github.com/gh-aw/reference/custom-safe-outputs/) |
| Triggering CI on agent-created PRs | `github-token-for-extra-empty-commit:` on `create-pull-request` | [Triggering CI](https://github.github.com/gh-aw/reference/triggering-ci/) |
| No guard against agent approving PRs | `allowed-events: [COMMENT, REQUEST_CHANGES]` on `submit-pull-request-review` | [Safe Outputs](https://github.github.com/gh-aw/reference/safe-outputs/) |

**Note:** gh-aw is actively developed. If a capability feels like something a framework would provide natively, check the reference docs — it probably exists even if it's not in this table yet.

For full architecture, security, fork handling, safe outputs, and troubleshooting details, read `references/architecture.md` in this skill directory.

## Common Patterns

### Pre-Agent Data Prep (the `steps:` pattern)

Use `steps:` for any operation requiring GitHub API access that the agent needs:

```yaml
steps:
  - name: Fetch PR data
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      gh pr view "$PR_NUMBER" --json title,body > pr-metadata.json
      gh pr diff "$PR_NUMBER" --name-only > changed-files.txt
```

### Safe Outputs (Posting Comments)

```yaml
safe-outputs:
  add-comment:
    max: 1
    hide-older-comments: true
    target: "*"    # Required for workflow_dispatch (no triggering PR context)
```

### Concurrency

Include all trigger-specific PR number sources:

```yaml
concurrency:
  group: "my-workflow-${{ github.event.issue.number || github.event.pull_request.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true
```

### Noise Reduction

Filter `pull_request` triggers to relevant paths and add a gate step:

```yaml
on:
  pull_request:
    paths:
      - 'src/**/tests/**'

steps:
  - name: Gate — skip if no relevant files
    if: github.event_name == 'pull_request'
    run: |
      FILES=$(gh pr diff "$PR_NUMBER" --name-only | grep -E '\.cs$' || true)
      if [ -z "$FILES" ]; then exit 1; fi
```

Manual triggers (`workflow_dispatch`, `issue_comment`) should bypass the gate. Note: `exit 1` causes a red ❌ on non-matching PRs — this is intentional (no built-in "skip" mechanism in gh-aw steps).

### Fork PR Checkout (workflow_dispatch)

For `workflow_dispatch` workflows that need to evaluate a PR branch: use the shared `Checkout-GhAwPr.ps1` script. It (1) verifies the PR author has write access and rejects fork PRs, (2) checks out the PR branch, and (3) restores `.github/skills/`, `.github/instructions/`, and `.github/copilot-instructions.md` from the base branch SHA — defense-in-depth even though the platform also does this restore automatically.

```yaml
steps:
  - name: Checkout PR and restore agent infrastructure
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

For `pull_request` + fork support (not `workflow_dispatch`): add `forks: ["*"]` to the trigger frontmatter. The platform automatically preserves `.github/` and `.agents/` as a base-branch artifact in the activation job, then restores them after `checkout_pr_branch.cjs` — fork PRs cannot overwrite agent infrastructure (gh-aw#23769, resolved).

### Security-Critical Patterns

These four patterns are the most commonly missed when building secure workflows. Use all where applicable:

**1. Prevent accidental PR approvals** — always restrict review workflows; otherwise the agent can approve PRs and bypass branch protection rules (gh-aw#25439):

```yaml
safe-outputs:
  submit-pull-request-review:
    allowed-events: [COMMENT, REQUEST_CHANGES]  # Blocks APPROVE at infrastructure level
```

**2. CI triggering + protected file safety** for agent-created PRs — `GITHUB_TOKEN` pushes don't trigger CI; a PAT/App token is required. `protected-files` controls what happens when the agent modifies package manifests or `.github/`:

```yaml
safe-outputs:
  create-pull-request:
    github-token-for-extra-empty-commit: ${{ secrets.PAT_OR_APP_TOKEN }}  # Required to trigger CI
    protected-files: fallback-to-issue   # Create issue instead of failing if agent touches .github/ or package manifests
    # protected-files: blocked (default) | allowed (disables protection)
```

**3. Filter untrusted content before the agent sees it** — prevents prompt injection from issue comments or PR descriptions authored by first-timers or contributors:

```yaml
tools:
  github:
    min-integrity: approved   # Filters FIRST_TIMER / CONTRIBUTOR content; use on workflows that process external PR content
```

**4. Fork PR checkout for `workflow_dispatch`** — the platform's `checkout_pr_branch.cjs` is skipped for `workflow_dispatch`, so you **must** use `.github/scripts/Checkout-GhAwPr.ps1` to check out the PR branch, verify write access, reject fork PRs, and restore trusted `.github/` from the base branch. Without it, the agent evaluates the workflow branch instead of the PR:

```yaml
steps:
  - name: Checkout PR and restore agent infrastructure
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

### Frontmatter Features

```yaml
source: "githubnext/agentics/workflows/ci-doctor.md@v1.0.0"  # Track workflow origin
private: true                                                    # Prevent installation via gh aw add
resources:                                                       # Companion files fetched with gh aw add
  - triage-issue.md
  - shared/helper-action.yml
labels: ["automation", "ci"]                                     # For gh aw status --label filtering

runtimes:                    # Override default runtime versions
  dotnet:
    version: "9.0"
  node:
    version: "22"

imports:                     # APM package dependencies
  - uses: shared/apm.md
    with:
      packages:
        - microsoft/apm-sample-package
```

Supported runtimes: `node`, `python`, `go`, `uv`, `bun`, `deno`, `ruby`, `java`, `dotnet`, `elixir`.

## When to Read the Full Reference

Read `.github/skills/gh-aw-guide/references/architecture.md` when you need:
- **Execution model** details (step ordering, credential availability, pre-agent-steps/post-steps)
- **Security boundaries** (defense layers, integrity filtering, protected files, rules for authors)
- **Fork PR handling** (platform restore, threat model, trigger-by-trigger behavior)
- **Safe outputs** (complete list of 30+ types, key options for each)
- **Troubleshooting** specific errors
- **Upstream issue history** (all 5 tracked issues and their resolutions)
