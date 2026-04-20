---
# Shared configuration for expert-review workflows.
#
# Imported by expert-review.md (slash command). Keeps permissions,
# tools, and safe-outputs in one place so all review entry points
# share the same behavior.

description: "Shared configuration for expert-review workflows"

permissions:
  contents: read
  pull-requests: read

tools:
  github:
    toolsets: [pull_requests, repos]

safe-outputs:
  create-pull-request-review-comment:
    max: 30
  submit-pull-request-review:
    max: 1
    allowed-events: [COMMENT]
  add-comment:
    max: 5
    hide-older-comments: true
---

# Expert Code Review

Review pull request #${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }} using the `expert-reviewer` agent defined at `.github/agents/expert-reviewer.agent.md`.

> **No test messages.** Never call any safe-output tool with placeholder or test content. Every call posts permanently on the PR. This applies to you and all sub-agents.

## Instructions

You are the orchestrator. Your job is to dispatch **3 parallel expert-reviewer sub-agents** with different models, collect their results, synthesize a consensus, and post the final review. Follow these steps exactly.

### Step 1: Gather Context

Fetch the PR diff and save it — you will pass it to each sub-agent:

```
gh pr diff <number>
gh pr view <number> --json title,body
gh pr checks <number>
```

### Step 2: Dispatch 3 Parallel Expert Reviewers

Launch **exactly 3 sub-agents in parallel** using the `task` tool. Each calls the `expert-reviewer` agent with a different model. All 3 must be launched — do not skip any.

```
task(agent_type: "general-purpose", model: "claude-opus-4.6", mode: "background",
     description: "Reviewer 1: deep reasoning review",
     prompt: "<full diff + PR description + instruction to follow .github/agents/expert-reviewer.agent.md>")

task(agent_type: "general-purpose", model: "claude-sonnet-4.6", mode: "background",
     description: "Reviewer 2: pattern matching review",
     prompt: "<same diff + same PR description + same instruction>")

task(agent_type: "general-purpose", model: "gpt-5.3-codex", mode: "background",
     description: "Reviewer 3: alternative perspective review",
     prompt: "<same diff + same PR description + same instruction>")
```

Each sub-agent prompt must include:
- The full PR diff
- The PR description
- This instruction: "You are an expert .NET MAUI code reviewer. Read and follow `.github/agents/expert-reviewer.agent.md` in this repo. Apply all review dimensions from that file. Return your findings as a structured list with severity, file, line, scenario, finding, and recommendation for each issue. Do NOT call any safe-output tools — just return your findings as text. Do NOT emit test messages."

**Wait for all 3 to complete before proceeding.**

### Step 3: Adversarial Consensus

Collect findings from all 3 sub-agents and apply consensus:

1. **3/3 agree** on a finding → include immediately
2. **2/3 agree** → include with median severity
3. **Only 1/3 flagged** → dispatch 2 follow-up sub-agents (the other 2 models) asking: "Reviewer X found this issue: [finding]. Do you agree or disagree? Explain why."
   - If 2+ now agree → include
   - If still 1/3 → discard (note as "discarded — single reviewer only")

### Step 4: Validate Paths and Line Numbers

Before posting inline comments, validate **both**:
1. **Path**: Run `gh pr diff <number> --name-only` to get the list of files in the diff. Only files in this list can receive inline comments. Comments on other files fail with "Path could not be resolved".
2. **Line**: Parse `@@ -old,len +new,len @@` — the line must be in `[new, new+len)`. Lines outside any hunk fail with "Line could not be resolved".

**If either path or line is invalid**, move the finding to `add_comment` (design-level) instead. A single invalid inline comment causes the entire `submit_pull_request_review` to fail and ALL inline comments are lost.

### Step 5: Post Results

1. **Inline comments** — `create_pull_request_review_comment` for findings where BOTH path and line are validated. Include "Flagged by: X/3 reviewers" in each.
2. **Design-level comment** — `add_comment` for findings outside the diff (one comment, multiple bullets).
3. **Final verdict** — `submit_pull_request_review` with:
   - Summary of all findings ranked by severity
   - Methodology note: "3 independent reviewers with adversarial consensus"
   - CI status, test coverage assessment, prior review status
   - Never mention specific model names — use "Reviewer 1/2/3"
   - `event: "COMMENT"` always — severity is communicated via emoji markers in the body, not the review event type. (Using `REQUEST_CHANGES` causes stale blocking reviews that can't be dismissed — see Known Limitations below.)
   - **Never use APPROVE**

### Known Limitation: Stale Blocking Reviews

gh-aw does not support `dismiss-pull-request-review` as a safe output, and workflows run with `pull-requests: read` (write is rejected by the compiler). If `REQUEST_CHANGES` were used, a stale blocking review from `github-actions[bot]` would persist even after findings are fixed, requiring manual dismissal. For this reason, all reviews use `COMMENT` event type — severity is expressed via markers in the review body, not the GitHub review state.
