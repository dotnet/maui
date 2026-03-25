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

By default, `gh aw compile` automatically injects a fork guard into the activation job's `if:` condition: `head.repo.id == repository_id`. This blocks fork PRs on `pull_request` events.

To **allow fork PRs**, add `forks: ["*"]` to the `pull_request` trigger in the `.md` frontmatter. The compiler removes the auto-injected guard from the compiled `if:` conditions. This is safe when the workflow uses the `Checkout-GhAwPr.ps1` pattern (checkout + trusted-infra restore) and the agent is sandboxed.

## Fork PR Handling

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials. Checkout alone is not dangerous — the vulnerability is executing workspace scripts with `GITHUB_TOKEN`.

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Fork PR Behavior by Trigger

| Trigger | `checkout_pr_branch.cjs` runs? | Fork handling |
|---------|-------------------------------|---------------|
| `pull_request` (default) | ✅ Yes | Blocked by auto-generated activation gate unless `forks: ["*"]` is set |
| `pull_request` + `forks: ["*"]` | ✅ Yes | ✅ Works — user steps restore trusted infra before agent runs |
| `workflow_dispatch` | ❌ Skipped | ✅ Works — user steps handle checkout and restore is final |
| `issue_comment` (same-repo) | ✅ Yes | ✅ Works — files already on PR branch |
| `issue_comment` (fork) | ✅ Yes | ⚠️ Works — `checkout_pr_branch.cjs` re-checks out fork branch after user steps, potentially overwriting restored infra. Acceptable because agent is sandboxed (no credentials, max 1 comment via safe-outputs). Pre-flight check catches missing `SKILL.md` if fork isn't rebased. |

### The `issue_comment` + Fork Problem

For `/slash-command` triggers on fork PRs, `checkout_pr_branch.cjs` runs AFTER all user steps and re-checks out the fork branch. This overwrites any files restored by user steps (e.g., `.github/skills/`). A fork could include a crafted `SKILL.md` that alters the agent's evaluation behavior.

**Accepted residual risk:** The agent runs in a sandboxed container with all credentials scrubbed. The worst outcome is a manipulated evaluation comment (`safe-outputs: add-comment: max: 1`). The agent has no ability to push code, access secrets, or exfiltrate data. The pre-flight check in the agent prompt catches the case where `SKILL.md` is missing entirely (fork not rebased on `main`).

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
2. Checks out the PR branch via `gh pr checkout`
3. Deletes `.github/skills/` and `.github/instructions/` (prevents fork-added files)
4. Restores them from the base branch SHA (best-effort, non-fatal)

**Behavior by trigger:**
- **`workflow_dispatch`**: Platform checkout is skipped, so the restore IS the final workspace state (trusted files from base branch)
- **`pull_request`** (same-repo): User step restores trusted infra. `checkout_pr_branch.cjs` runs after and re-checks out PR branch — for same-repo PRs, skill files typically match main unless the PR modified them.
- **`pull_request`** (fork with `forks: ["*"]`): Same as above, but fork's skill files may differ. Same residual risk as `issue_comment` fork case — agent is sandboxed, pre-flight catches missing `SKILL.md`.
- **`issue_comment`** (same-repo): Platform re-checks out PR branch — files already match, effectively a no-op
- **`issue_comment`** (fork): Platform re-checks out fork branch after us, overwriting restored files. Agent is sandboxed; pre-flight in the prompt catches missing `SKILL.md`

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
| User steps always before platform steps | Cannot run user code after `checkout_pr_branch.cjs` | For `issue_comment` fork PRs, accept sandboxed residual risk; see [gh-aw#18481](https://github.com/github/gh-aw/issues/18481) |
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
| Agent can't find SKILL.md | Fork PR branch doesn't include `.github/skills/` | Rebase fork on `main`, or use `workflow_dispatch` with `pr_number` input |
| Fork PR skipped on `pull_request` | `forks: ["*"]` not in workflow frontmatter | Add `forks: ["*"]` under `pull_request:` in the `.md` source and recompile |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Integrity filtering warning | MCP reading fork PR data | Expected, non-blocking |
| `/slash-command` doesn't trigger | Workflow not on default branch | Merge to `main` first |
