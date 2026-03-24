---
applyTo:
  - ".github/workflows/*.md"
  - ".github/workflows/*.lock.yml"
---

# gh-aw (GitHub Agentic Workflows) Guidelines

## Architecture

gh-aw workflows are authored as `.md` files with YAML frontmatter, compiled to `.lock.yml` via `gh aw compile`. The lock file is auto-generated — **never edit it manually**.

### Execution Model

```
activation job  (renders prompt from base branch .md via runtime-import)
    ↓
agent job:
  user steps:       (pre-agent, OUTSIDE firewall, has GITHUB_TOKEN)
    ↓
  platform steps:   (configure git → checkout_pr_branch.cjs → install CLI)
    ↓
  agent:            (INSIDE sandboxed container, NO credentials)
```

| Context | Has GITHUB_TOKEN | Has gh CLI | Has git creds | Can execute scripts |
|---------|-----------------|-----------|---------------|-------------------|
| `steps:` (user) | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — **be careful** |
| Platform steps | ✅ Yes | ✅ Yes | ✅ Yes | Platform-controlled |
| Agent container | ❌ Scrubbed | ❌ Scrubbed | ❌ Scrubbed | ✅ But sandboxed |

### Step Ordering (Critical)

User `steps:` **always run before** platform-generated steps. You cannot insert user steps after platform steps.

The platform's `checkout_pr_branch.cjs` runs with `if: (github.event.pull_request) || (github.event.issue.pull_request)` — it is **skipped** for `workflow_dispatch` triggers.

### Prompt Rendering

The prompt is built in the **activation job** via `{{#runtime-import .github/workflows/<name>.md}}`. This reads the `.md` file from the **base branch** workspace (before any PR checkout). The rendered prompt is uploaded as an artifact and downloaded by the agent job.

- The agent prompt is always the base branch version — fork PRs cannot alter it
- The prompt references files on disk (e.g., `SKILL.md`) — those files must exist in the agent's workspace

### Fork PR Activation Gate

`gh aw compile` automatically injects a fork guard into the activation job's `if:` condition: `head.repo.id == repository_id`. This blocks fork PRs on `pull_request` events. This is **platform behavior** — do not add it manually.

## Fork PR Handling

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous — the vulnerability is executing workspace scripts with `GITHUB_TOKEN`.

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Fork PR Behavior by Trigger

| Trigger | `checkout_pr_branch.cjs` runs? | Fork handling |
|---------|-------------------------------|---------------|
| `pull_request` | ✅ Yes | Blocked by auto-generated activation gate |
| `workflow_dispatch` | ❌ Skipped | ✅ Works — user steps handle checkout and restore is final |
| `issue_comment` (same-repo) | ✅ Yes | ✅ Works — files already on PR branch |
| `issue_comment` (fork) | N/A | ❌ Blocked by fail-closed fork guard in `Checkout-GhAwPr.ps1` |

### The `issue_comment` + Fork Problem

For `/slash-command` triggers on fork PRs, `checkout_pr_branch.cjs` runs AFTER all user steps and re-checks out the fork branch. This overwrites any files restored by user steps (e.g., `.github/skills/`). There is no way to run user steps after platform steps. A fork could include a crafted `SKILL.md` that alters the agent's evaluation behavior.

**Current approach (fail-closed fork guard):** `Checkout-GhAwPr.ps1` checks `isCrossRepository` via `gh pr view` for `issue_comment` triggers. If the PR is from a fork or the API call fails, the script exits with code 1. Fork PRs should use `workflow_dispatch` instead, where `checkout_pr_branch.cjs` is skipped and the user step restore is the final workspace state.

**Upstream issue:** [github/gh-aw#18481](https://github.com/github/gh-aw/issues/18481) — "Using gh-aw in forks of repositories"

### Safe Pattern: Checkout + Restore

Use the shared `.github/scripts/Checkout-GhAwPr.ps1` script, which implements checkout + restore in a single reusable step:

```yaml
steps:
  - name: Checkout PR and restore agent infrastructure
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
```

The script:
1. Captures the base branch SHA before checkout
2. **Fork guard** (`issue_comment` only): checks `isCrossRepository` — exits 1 if fork or API failure
3. Checks out the PR branch via `gh pr checkout`
4. Deletes `.github/skills/` and `.github/instructions/` (prevents fork-added files)
5. Restores them from the base branch SHA (best-effort, non-fatal)

**Behavior by trigger:**
- **`workflow_dispatch`**: Fork guard skipped. Platform checkout is skipped, so the restore IS the final workspace state (trusted files from base branch)
- **`issue_comment`** (same-repo): Fork guard passes. Platform re-checks out PR branch — files already match, effectively a no-op
- **`issue_comment`** (fork): Fork guard rejects — exits 1 with actionable notice to use `workflow_dispatch`
- **`pull_request`** (same-repo): Fork guard skipped. Files already exist, restore is a no-op

### Anti-Patterns

**Do NOT skip checkout for fork PRs:**

```bash
# ❌ ANTI-PATTERN: Makes fork PRs unevaluable
if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
  echo "Skipping checkout for fork PR"
  exit 0  # Agent evaluates workflow branch instead of PR
fi
```

Skipping checkout means the agent evaluates the wrong files. The correct approach is: always check out the PR, then restore agent infrastructure from the base branch.

**Do NOT execute workspace code after fork checkout:**

```yaml
# ❌ DANGEROUS: runs fork code with GITHUB_TOKEN
- name: Checkout PR
  run: gh pr checkout "$PR_NUMBER" ...
- name: Run analysis
  run: pwsh .github/skills/some-script.ps1
```

If you need to run scripts, either:
1. Run them **before** the checkout (from the base branch)
2. Run them **inside the agent container** (sandboxed, no tokens)

## Compilation

```bash
# Compile after every change to the .md source
gh aw compile .github/workflows/<name>.md

# This updates:
# - .github/workflows/<name>.lock.yml (auto-generated)
# - .github/aw/actions-lock.json
```

**Always commit the compiled lock file alongside the source `.md`.**

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

## Limitations

| What | Behavior | Workaround |
|------|----------|------------|
| User steps always before platform steps | Cannot run user code after `checkout_pr_branch.cjs` | Use `workflow_dispatch` for fork PRs; see [gh-aw#18481](https://github.com/github/gh-aw/issues/18481) |
| `--allow-all-tools` in lock.yml | Emitted by `gh aw compile` | Cannot override from `.md` source |
| MCP integrity filtering | Fork PRs blocked as "unapproved" | Use `steps:` checkout instead of MCP |
| `gh` CLI inside agent | Credentials scrubbed | Use `steps:` for API calls, or MCP tools |
| `issue_comment` trigger | Requires workflow on default branch | Must merge to `main` before `/slash-commands` work |
| Duplicate runs | gh-aw sometimes creates 2 runs per dispatch | Harmless, use concurrency groups |

### Upstream References

- [github/gh-aw#18481](https://github.com/github/gh-aw/issues/18481) — Fork support tracking issue
- [github/gh-aw#18518](https://github.com/github/gh-aw/issues/18518) — Fork detection in `gh aw init`
- [github/gh-aw#18521](https://github.com/github/gh-aw/issues/18521) — Fork support documentation

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Agent evaluates wrong PR | `workflow_dispatch` checks out workflow branch | Add `gh pr checkout` in `steps:` |
| Agent can't find SKILL.md | Fork PR branch doesn't have `.github/skills/` | Agent posts "rebase or use `workflow_dispatch`" message; or rebase fork on `main` |
| Fork PR rejected on `/evaluate-tests` | Fail-closed fork guard in `Checkout-GhAwPr.ps1` | Use `workflow_dispatch` with `pr_number` input instead |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Integrity filtering warning | MCP reading fork PR data | Expected, non-blocking |
| `/slash-command` doesn't trigger | Workflow not on default branch | Merge to `main` first |
