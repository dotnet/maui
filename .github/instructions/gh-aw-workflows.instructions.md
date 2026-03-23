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
steps:  (pre-agent, OUTSIDE firewall, has GITHUB_TOKEN)
   ↓
gh-aw platform  (copies workspace into container, scrubs credentials)
   ↓
agent  (INSIDE sandboxed container, NO credentials)
```

| Context | Has GITHUB_TOKEN | Has gh CLI | Has git creds | Can execute scripts |
|---------|-----------------|-----------|---------------|-------------------|
| `steps:` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes — **be careful** |
| Agent container | ❌ Scrubbed | ❌ Scrubbed | ❌ Scrubbed | ✅ But sandboxed |

### Key Principle: `steps:` is the Privilege Boundary

The `steps:` section runs with full token access. The agent container is sandboxed. This separation is the security model.

## Fork PR Security

### The "pwn-request" Threat Model

The classic attack requires **checkout + execution** of fork code with elevated credentials:
1. Fork code checked out onto disk ✅ (checkout alone is not dangerous)
2. A step **runs** a script from that code (e.g., `npm install`, `./build.sh`) with GITHUB_TOKEN ❌ (THIS is the vulnerability)

Reference: https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

### Safe Pattern: Checkout Without Execution

```yaml
steps:
  - name: Checkout PR branch
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || inputs.pr_number }}
    run: |
      # Safe: checkout only, no workspace scripts executed
      gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"
```

This is safe for fork PRs because:
- `gh pr checkout` is a GitHub CLI command, not workspace code
- No subsequent step executes anything from the checked-out workspace
- The agent runs in a sandboxed container with scrubbed credentials

### 🚨 NEVER Do This After Fork Checkout

```yaml
steps:
  - name: Checkout PR
    run: gh pr checkout "$PR_NUMBER" ...

  # ❌ DANGEROUS: runs workspace code with GITHUB_TOKEN
  - name: Run analysis
    run: pwsh .github/skills/some-script.ps1
```

If you need to run scripts, either:
1. Run them **before** the checkout (from the base branch)
2. Run them **inside the agent container** (sandboxed, no tokens)

### Fork Guard Anti-Pattern

Do NOT add fork detection guards that skip checkout for fork PRs:

```yaml
# ❌ ANTI-PATTERN: Makes fork PRs unevaluable
if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
  echo "Skipping checkout for fork PR"
  exit 0
fi
```

This breaks the workflow for fork PRs because:
- MCP tools are blocked by integrity filtering for fork ("unapproved") PRs
- `gh` CLI doesn't work inside the agent container (credentials scrubbed)
- The agent ends up evaluating the workflow branch instead of the PR

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

The agent can then read these files from its local workspace.

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

Manual triggers (`workflow_dispatch`, `issue_comment`) should bypass the gate.

## Limitations

| What | Behavior | Workaround |
|------|----------|------------|
| `--allow-all-tools` in lock.yml | Emitted by `gh aw compile` | Cannot override from `.md` source |
| MCP integrity filtering | Fork PRs blocked as "unapproved" | Use `steps:` checkout instead of MCP |
| `gh` CLI inside agent | Credentials scrubbed | Use `steps:` for API calls, or MCP tools |
| Duplicate runs | gh-aw sometimes creates 2 runs per dispatch | Harmless, use concurrency groups |

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Agent evaluates wrong PR | `workflow_dispatch` checks out workflow branch | Add `gh pr checkout` in `steps:` |
| Fork PR has no data | Fork guard skipping checkout + MCP blocked | Remove fork guard, allow checkout |
| `gh` commands fail in agent | Credentials scrubbed inside container | Move to `steps:` section |
| Lock file out of date | Forgot to recompile | Run `gh aw compile` |
| Integrity filtering warning | MCP reading fork PR data | Expected, non-blocking |
