# Configuring a gh-aw Workflow That Creates PRs Securely

## Problem Statement

You want a gh-aw workflow where the agent produces code changes and opens a PR, with three requirements:

1. **Created PRs must trigger CI** (`pull_request` workflows fire on the new PR)
2. **Agent cannot modify `.github/`** (protecting workflow files, skills, instructions)
3. **Agent cannot approve its own PRs** (human review required before merge)

---

## TL;DR Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Activation job       (renders prompt, triggers workflow)       │
│                                                                 │
│  Agent job:                                                     │
│    steps:             (pre-agent, HAS GITHUB_TOKEN)             │
│      ↓ checkout, data prep, app-token generation                │
│    platform steps:    (git config, CLI install)                  │
│      ↓                                                          │
│    agent container:   (sandboxed, NO credentials)               │
│      ↓ makes code changes, writes to workspace                 │
│                                                                 │
│  safe-outputs job:    (separate job, write permissions)         │
│    create-pull-request: creates PR using app token              │
│      ↓ excludes .github/ from the PR diff                      │
│      ↓ PR triggers CI because app token ≠ GITHUB_TOKEN         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1. CI Triggering: Use a GitHub App Token

### The Problem

PRs created with `GITHUB_TOKEN` **do not** trigger `pull_request` or `push` workflows. This is a deliberate GitHub security measure to prevent recursive workflow runs. If your safe-output creates a PR with `GITHUB_TOKEN`, CI will never start on that PR.

### The Solution

Use a **GitHub App installation token** to create the PR. PRs authored by a GitHub App *do* trigger workflows.

```yaml
steps:
  - name: Generate app token for PR creation
    id: app-token
    uses: actions/create-github-app-token@v1
    with:
      app-id: ${{ vars.PR_CREATOR_APP_ID }}
      private-key: ${{ secrets.PR_CREATOR_APP_PRIVATE_KEY }}
```

Then pass that token to the safe-outputs job (see full example below).

### Why Not a PAT?

A Personal Access Token (PAT) also triggers CI, but it's tied to a *person*. If that person leaves the org, the token breaks. A GitHub App is a machine identity — it survives personnel changes and has fine-grained permission scoping.

### GitHub App Minimum Permissions

The GitHub App used for PR creation needs:

| Permission | Level | Why |
|------------|-------|-----|
| **Contents** | Write | Push the branch with changes |
| **Pull requests** | Write | Create the PR |
| **Metadata** | Read | Required for all apps |

**Do NOT grant**: `Administration`, `Actions`, `Workflows`, or any other permission. Minimal scope limits blast radius.

---

## 2. Protecting `.github/` — Defense in Depth

No single mechanism is sufficient. Use **all three layers**:

### Layer 1: Repository Rulesets (Primary Guard)

GitHub repository rulesets can enforce that changes to `.github/**` require review from specific teams. This is the strongest protection because it's enforced server-side by GitHub and cannot be bypassed by any token.

Go to **Settings → Rules → Rulesets** and create a ruleset:

```
Name: Protect workflow and agent infrastructure
Enforcement: Active
Target: Default branch + all branches matching "agent/*"

Rules:
  - Restrict file paths:
    - Protected paths: .github/**
  - Require pull request reviews:
    - Required approvals: 1
    - Dismiss stale reviews on new pushes: true
    - Require review from code owners: true
```

This means even if the agent *does* include `.github/` changes in its commit, the PR cannot be merged without a human reviewing those specific paths.

### Layer 2: CODEOWNERS (Review Requirement)

Add or verify a CODEOWNERS entry that requires specific team review for `.github/`:

```
# .github/CODEOWNERS (excerpt)
/.github/                @dotnet/maui-engineering
/.github/workflows/      @dotnet/maui-engineering
/.github/skills/         @dotnet/maui-engineering
/.github/instructions/   @dotnet/maui-engineering
```

When combined with branch protection requiring CODEOWNERS approval, this ensures a human from the engineering team must sign off on any `.github/` changes.

### Layer 3: Pre-Agent Step to Strip `.github/` Changes

Add a step in the workflow that runs *after* the agent but *before* PR creation, which strips any `.github/` modifications from the agent's working tree. Since the agent runs in a sandboxed container with no credentials, all its filesystem changes are captured and then processed by the safe-outputs job. You can intervene in a `steps:` section:

```yaml
steps:
  - name: Strip .github/ changes from agent output
    run: |
      # Restore .github/ to the base branch state
      # This runs BEFORE the safe-outputs job creates the PR
      git checkout HEAD -- .github/ 2>/dev/null || true
      echo "✅ Ensured .github/ directory is unchanged"
```

> **Note on ordering**: User `steps:` run *before* platform steps and the agent. To strip changes *after* the agent, you would need this logic in a post-processing mechanism. In practice, Layers 1 and 2 are your enforcement — Layer 3 is a convenience to keep the PR diff clean. If gh-aw's `create-pull-request` safe-output supports an `exclude-paths` option, prefer that (check the latest gh-aw reference docs).

---

## 3. Preventing Self-Approval

### The Problem

If the same identity (token/app) that creates the PR also approves it, you have no human oversight. The agent must not be able to approve or merge its own PRs.

### Solution A: Branch Protection Rules

Configure branch protection on your target branch (e.g., `main`):

```
Settings → Branches → Branch protection rules → main:
  ✅ Require a pull request before merging
    - Required number of approving reviews: 1
    - ✅ Dismiss stale pull request approvals when new commits are pushed
    - ✅ Require review from Code Owners
  ✅ Require status checks to pass before merging
    - Required checks: maui-pr (or your CI pipeline name)
  ✅ Do not allow bypassing the above settings
```

**Key**: GitHub already prevents the PR author from approving their own PR when "Require pull request reviews" is enabled. Since the GitHub App is the PR author, neither the app nor the agent can approve it.

### Solution B: Never Use `--approve` in gh-aw Workflows

From the repository's own conventions:

> 🚨 CRITICAL: NEVER use `--approve` or `--request-changes` — only post comments. Approval is a human decision.

In your workflow's prompt/instructions to the agent, explicitly state:

```markdown
## Rules
- You MUST NOT approve pull requests
- You MUST NOT use `gh pr review --approve`
- You MUST NOT merge pull requests
- You may only add comments to PRs using safe-outputs
```

### Solution C: GitHub App Permission Scoping

The GitHub App used for PR creation should **not** have permission to approve PRs. Even if `Pull requests: Write` allows creating review comments, GitHub's built-in rule prevents the PR author from self-approving when branch protection is on.

For extra safety, create *two* GitHub Apps with separated concerns:

| App | Permissions | Used For |
|-----|-------------|----------|
| `maui-agent-pr-creator` | Contents: Write, Pull requests: Write | Creating the PR branch and PR |
| `maui-agent-commenter` | Pull requests: Read, Issues: Write | Posting status comments (if needed) |

The PR-creator app can never approve because it's the author. The commenter app doesn't have write PR permissions.

---

## Full Workflow Example

```yaml
# .github/workflows/agent-code-changes.md
---
name: Agent Code Changes
description: Agent analyzes an issue and creates a PR with a fix

on:
  workflow_dispatch:
    inputs:
      issue_number:
        description: "GitHub issue number to fix"
        required: true

  # Optional: trigger on issue label
  # label_command:
  #   - name: agent-fix
  #     on: issues

concurrency:
  group: "agent-fix-${{ inputs.issue_number || github.event.issue.number || github.run_id }}"
  cancel-in-progress: true

# Reaction emoji when triggered (built-in gh-aw feature)
reaction: rocket

safe-outputs:
  create-pull-request:
    max: 1
    base: main
    # If gh-aw supports path exclusions:
    # exclude-paths:
    #   - ".github/**"
  add-comment:
    max: 1
    target: "${{ inputs.issue_number || github.event.issue.number }}"

steps:
  # Step 1: Generate a GitHub App token for PR creation
  # This token (not GITHUB_TOKEN) ensures the PR triggers CI
  - name: Generate App Token
    id: app-token
    uses: actions/create-github-app-token@v1
    with:
      app-id: ${{ vars.AGENT_APP_ID }}
      private-key: ${{ secrets.AGENT_APP_PRIVATE_KEY }}

  # Step 2: Gather issue context (runs with GITHUB_TOKEN, before agent)
  - name: Fetch issue details
    env:
      GH_TOKEN: ${{ github.token }}
      ISSUE_NUMBER: ${{ inputs.issue_number || github.event.issue.number }}
    run: |
      gh issue view "$ISSUE_NUMBER" --json title,body,labels,comments > issue-context.json
      echo "Issue context saved for agent"

  # Step 3: Create a working branch
  - name: Create agent branch
    env:
      GH_TOKEN: ${{ steps.app-token.outputs.token }}
      ISSUE_NUMBER: ${{ inputs.issue_number || github.event.issue.number }}
    run: |
      BRANCH="agent/fix-issue-${ISSUE_NUMBER}"
      git checkout -b "$BRANCH"
      echo "AGENT_BRANCH=$BRANCH" >> "$GITHUB_ENV"

  # Step 4: Protect .github/ - save a manifest of expected state
  - name: Save .github/ baseline
    run: |
      find .github -type f -exec sha256sum {} \; | sort > /tmp/github-dir-baseline.txt
      echo "✅ .github/ baseline captured"
---

# Agent Prompt

You are a code-fixing agent for the dotnet/maui repository.

## Your Task
Read the issue context from `issue-context.json` and implement a fix.

## Rules — READ CAREFULLY
1. **DO NOT modify any files under `.github/`** — this directory contains workflow
   definitions, skills, and instructions that are protected. Changes to `.github/`
   will be stripped from your PR.
2. **DO NOT approve or merge pull requests.** You create PRs; humans approve them.
3. **DO NOT commit secrets, tokens, or credentials.**
4. Follow existing code conventions (see `.github/copilot-instructions.md`).
5. Run `dotnet format` before finalizing changes.
6. Include a clear PR description explaining what you changed and why.

## Workflow
1. Read `issue-context.json` to understand the issue
2. Explore the codebase to find relevant files
3. Implement the fix
4. Run `dotnet format` on changed files
5. Use `create-pull-request` to open a PR with your changes
6. Use `add-comment` to post a summary on the original issue
```

### Compiled Lock File

After writing the `.md`, compile it:

```bash
gh aw compile .github/workflows/agent-code-changes.md
```

This generates `.github/workflows/agent-code-changes.lock.yml`. **Always commit both files.**

---

## Security Model Summary

| Threat | Mitigation | Layer |
|--------|-----------|-------|
| PR doesn't trigger CI | Use GitHub App token (not `GITHUB_TOKEN`) for PR creation | Token management |
| Agent modifies `.github/` | Repository rulesets protecting `.github/**` paths | Server-side enforcement |
| Agent modifies `.github/` | CODEOWNERS requiring engineering team review | Review policy |
| Agent modifies `.github/` | Explicit instructions in agent prompt forbidding it | Agent behavior |
| Agent modifies `.github/` | Pre-PR step stripping `.github/` changes (if feasible) | Workflow step |
| Agent approves own PR | Branch protection: author cannot self-approve | Server-side enforcement |
| Agent approves own PR | GitHub App has no approval permission path | Permission scoping |
| Agent approves own PR | Explicit instructions + never use `--approve` | Agent behavior |
| Agent merges without review | Branch protection: require status checks + review | Server-side enforcement |
| Recursive workflow triggers | `GITHUB_TOKEN` PRs don't trigger (by design); App token PRs do trigger but concurrency groups prevent loops | GitHub platform + concurrency |

### Defense-in-Depth Principle

Never rely on a single layer. The agent prompt says "don't modify `.github/`", but prompts are probabilistic — the agent *might* ignore them. Repository rulesets are deterministic and cannot be bypassed. Use both.

```
Agent prompt (behavioral)     → "Don't touch .github/"
↓ (agent might ignore)
Workflow step (procedural)    → Strip .github/ changes before PR
↓ (might not catch everything)
CODEOWNERS (review policy)    → Require team review for .github/
↓ (reviewer might miss it)
Repository ruleset (enforced) → Block merge without approved .github/ review
```

---

## Checklist Before Deploying

- [ ] GitHub App created with minimal permissions (Contents: Write, Pull requests: Write, Metadata: Read)
- [ ] App ID stored in `vars.AGENT_APP_ID`, private key in `secrets.AGENT_APP_PRIVATE_KEY`
- [ ] Repository ruleset protecting `.github/**` is active
- [ ] CODEOWNERS includes `.github/` entries with appropriate team
- [ ] Branch protection requires PR reviews (≥1 approval, dismiss stale, require CODEOWNERS)
- [ ] Branch protection requires status checks (CI must pass)
- [ ] Workflow `.md` compiled with `gh aw compile`
- [ ] Both `.md` and `.lock.yml` committed
- [ ] Tested: PR created by workflow triggers CI pipeline
- [ ] Tested: PR with `.github/` changes cannot be merged without team review
- [ ] Tested: Agent/App cannot self-approve the PR

---

## Common Pitfalls

| Pitfall | Why It Happens | Fix |
|---------|----------------|-----|
| CI doesn't trigger on agent PR | PR created with `GITHUB_TOKEN` | Use GitHub App token for `create-pull-request` |
| Agent pushes `.github/` changes | No server-side path protection | Add repository ruleset for `.github/**` |
| Agent approves its own PR | Missing branch protection | Enable "Require pull request reviews" |
| Infinite workflow loop | Agent PR triggers this same workflow | Use concurrency groups + filter on `workflow_dispatch` only |
| App token expired mid-run | Long-running agent exceeds token TTL (1 hour default) | Keep agent task focused; regenerate if needed |
| Lock file out of date | Forgot to recompile after `.md` edit | Always run `gh aw compile` after changes |
